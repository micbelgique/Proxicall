// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using ProxiCall.Models;
using System.Linq;

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
        private readonly ProxiCallAccessors _accessors;
        private readonly ILogger _logger;
        private DialogSet _dialogs;
        public WaterfallStep[] waterfallSteps { get; set; }

        //Conversation steps
        private const string DetailsWaterfall = "details";
        private const string QuerySearchPrompt = "querysearch";
        private const string PhoneNumberPrompt = "phonenumber";
        private const string ConfirmCallPrompt = "confirm";

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public ProxiCallBot(ProxiCallAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<ProxiCallBot>();
            _logger.LogTrace("Turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            // The DialogSet needs a DialogState accessor, it will call it when it has a turn context.
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            waterfallSteps = new WaterfallStep[]
            {
                //steps
                QuerySearchStepAsync,
                ResponseToQueryStepAsync,
                ConfirmCallStepAsync,//Make a phone call if confirmed
                EndDialogStepsAsync,
            };
            _dialogs.Add(new WaterfallDialog(DetailsWaterfall, waterfallSteps));
            _dialogs.Add(new TextPrompt(QuerySearchPrompt));
            _dialogs.Add(new TextPrompt(PhoneNumberPrompt));
            _dialogs.Add(new ConfirmPrompt(ConfirmCallPrompt));
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

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync(DetailsWaterfall, null, cancellationToken);
                }
            }
            else
            {
                if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text(Properties.strings.welcome,
                                                    Properties.strings.welcome,
                                                    InputHints.IgnoringInput);
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }

            // Save the dialog state into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Save the user profile updates into the user state.
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private async Task<DialogTurnResult> QuerySearchStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(QuerySearchPrompt, new PromptOptions { Prompt = MessageFactory.Text(Properties.strings.querySearchPerson) }, cancellationToken);
        }

        private async Task<DialogTurnResult> ResponseToQueryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var searchedUser = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Update the profile.
            searchedUser.Name = (string)stepContext.Result;

            var phoneNumber = "0000"; //TODO remove hardcoded number

            // INSERT QUERY TO DATABASE HERE

            searchedUser.PhoneNumber = phoneNumber;
            await stepContext.Context
                .SendActivityAsync(MessageFactory
                .Text($"{Properties.strings.phoneNumberOf_1} {stepContext.Result} {Properties.strings.phoneNumberOf_2} : " + phoneNumber + "."), cancellationToken);

            return await stepContext.ContinueDialogAsync();
        }

        private async Task<DialogTurnResult> ConfirmCallStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchedUser = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            var response = new PromptOptions
            {
                Prompt = MessageFactory.Text(Properties.strings.forwardCallPrompt + $" - ({searchedUser.PhoneNumber})"),
                RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
            };
            return await stepContext.PromptAsync(ConfirmCallPrompt, response, cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialogStepsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchedUser = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            var confirmation = (bool)stepContext.Result;
            string msg = null;
            if (confirmation)
            {
                msg = $"{Properties.strings.callForwardingConfirmed} {searchedUser.Name} (demo).";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                //TODO ask for call forwarding
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(Properties.strings.goodbye), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
