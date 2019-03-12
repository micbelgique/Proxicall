// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Linq;
using ProxiCall.Dialogs.TelExchange;
using ProxiCall.Models.Intents;

namespace ProxiCall
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class ProxiCallBot : IBot
    {
        private const string LuisConfiguration = "proxicall-luis";

        private readonly BotServices _services;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;

        public DialogSet Dialogs { get; private set; }

        private readonly IStatePropertyAccessor<TelExchangeState> _telExchangeStateAccessor;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public ProxiCallBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new System.ArgumentException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            _telExchangeStateAccessor = _userState.CreateProperty<TelExchangeState>(nameof(TelExchangeState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            Dialogs = new DialogSet(_dialogStateAccessor);
            Dialogs.Add(new TelExchangeDialog(_telExchangeStateAccessor, _loggerFactory, _services));
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var activity = turnContext.Activity;

            // Create a dialog context
            var dialogContext = await Dialogs.CreateContextAsync(turnContext);

            if (activity.Type == ActivityTypes.Message)
            {
                // Perform a call to LUIS to retrieve results for the current activity message.
                var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(dialogContext.Context, cancellationToken);

                // If any entities were updated, treat as interruption.
                // For example, "no my name is tony" will manifest as an update of the name to be "tony".
                var topScoringIntent = luisResults?.GetTopScoringIntent();

                var topIntent = topScoringIntent.Value.intent;


                //TODO 
                //// Handle conversation interrupts first.
                //var interrupted = await IsTurnInterruptedAsync(dialogContext, topIntent);
                //if (interrupted)
                //{
                //    // Bypass the dialog.
                //    // Save state before the next turn.
                //    await _conversationState.SaveChangesAsync(turnContext);
                //    await _userState.SaveChangesAsync(turnContext);
                //    return;
                //}

                // Continue the current dialog
                var dialogResult = await dialogContext.ContinueDialogAsync();

                // if no one has responded,
                if (!dialogContext.Context.Responded)
                {
                    // examine results from active dialog
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            switch (topIntent)
                            {
                                case Intents.TelephoneExchange:
                                case Intents.MakeACall:
                                    // update call state with any entities captured
                                    await UpdateCallStateAsync(luisResults, topIntent, dialogContext.Context);

                                    await dialogContext.BeginDialogAsync(nameof(TelExchangeDialog));
                                    break;

                                case Intents.None:
                                default:
                                    // Help or no intent identified, either way, let's provide some help.
                                    // to the user
                                    await dialogContext.Context.SendActivityAsync(Properties.strings.noIntentError);
                                    break;
                            }

                            break;

                        case DialogTurnStatus.Waiting:
                            // The active dialog is waiting for a response from the user, so do nothing.
                            break;

                        case DialogTurnStatus.Complete:
                            await dialogContext.EndDialogAsync();
                            break;

                        default:
                            await dialogContext.CancelAllDialogsAsync();
                            break;
                    }
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                var reply = MessageFactory.Text(Properties.strings.welcome,
                                                Properties.strings.welcome,
                                                InputHints.AcceptingInput);
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }

            // Save the dialog state into the conversation state.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Save the user profile updates into the user state.
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Helper function to update greeting state with entities returned by LUIS.
        /// <param name="luisResult">LUIS recognizer <see cref="RecognizerResult"/>.</param>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// </summary>
        private async Task UpdateCallStateAsync(RecognizerResult luisResult, string intentName, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                // Get latest GreetingState
                var telExchangeState = await _telExchangeStateAccessor.GetAsync(turnContext, () => new TelExchangeState());
                var entities = luisResult.Entities;

                // Supported LUIS Entities
                string[] personNameEntities = { "personName" };

                // Update any entities
                // Note: Consider a confirm dialog, instead of just updating.
                foreach (var entity in personNameEntities)
                {
                    // Check if we found valid slot values in entities returned from LUIS.
                    if (entities[entity] != null)
                    {
                        // Capitalize and set new user name.
                        var fullName = (string)entities[entity][0];
                        telExchangeState.RecipientFullName = fullName;
                        break;
                    }
                }

                telExchangeState.IntentName = intentName;

                // Set the new values into state.
                await _telExchangeStateAccessor.SetAsync(turnContext, telExchangeState);
            }
        }

        //// Determine if an interruption has occurred before we dispatch to any active dialog.
        //private async Task<bool> IsTurnInterruptedAsync(DialogContext dc, string topIntent)
        //{
        //    // See if there are any conversation interrupts we need to handle.
        //    if (topIntent.Equals(CancelIntent))
        //    {
        //        if (dc.ActiveDialog != null)
        //        {
        //            await dc.CancelAllDialogsAsync();
        //            await dc.Context.SendActivityAsync("Ok. I've canceled our last activity.");
        //        }
        //        else
        //        {
        //            await dc.Context.SendActivityAsync("I don't have anything to cancel.");
        //        }

        //        return true;        // Handled the interrupt.
        //    }

        //    if (topIntent.Equals(HelpIntent))
        //    {
        //        await dc.Context.SendActivityAsync("Let me try to provide some help.");
        //        await dc.Context.SendActivityAsync("I understand greetings, being asked for help, or being asked to cancel what I am doing.");
        //        if (dc.ActiveDialog != null)
        //        {
        //            await dc.RepromptDialogAsync();
        //        }

        //        return true;        // Handled the interrupt.
        //    }

        //    return false;           // Did not handle the interrupt.
        //}
    }
}