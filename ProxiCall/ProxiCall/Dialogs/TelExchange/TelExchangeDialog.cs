using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using ProxiCall.Models.Intents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.TelExchange
{
    public class TelExchangeDialog : ComponentDialog
    {
        public IStatePropertyAccessor<TelExchangeState> TelExchangeStateAccessor { get; }
        public ILoggerFactory LoggerFactory { get; }
        public BotServices BotServices { get; }

        //Recipient state for TelExchange dialog
        private const string TelExchangeWaterfall = "tellExchangeWaterfall";
        private const string RecipientNamePrompt = "recipientNamePrompt";
        private const string RetryNumberSearchPrompt = "retryNumberSearchPrompt";
        private const string ConfirmForwardingPrompt = "confirmForwardingPrompt";
        private const string EndCallDialogPrompt = "endCallDialogPrompt";

        public TelExchangeDialog(IStatePropertyAccessor<TelExchangeState> telExchangeStateAccessor, ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(TelExchangeDialog))
        {
            TelExchangeStateAccessor = telExchangeStateAccessor;
            LoggerFactory = loggerFactory;
            BotServices = botServices;

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                PromptForRecipientNameStepAsync,
                SearchRecipientNumberStepAsync,
                ResultHandlerStepAsync,
                ForwardCallStepAsync,
                EndTelExchangeDialogStepAsync,
            };
            AddDialog(new WaterfallDialog(TelExchangeWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(RecipientNamePrompt));
            AddDialog(new ConfirmPrompt(RetryNumberSearchPrompt, defaultLocale: "fr-fr"));
            AddDialog(new ConfirmPrompt(ConfirmForwardingPrompt, defaultLocale: "fr-fr"));
            AddDialog(new ConfirmPrompt(EndCallDialogPrompt, defaultLocale: "fr-fr"));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var telExchangeState = await TelExchangeStateAccessor.GetAsync(stepContext.Context, () => null);
            if (telExchangeState == null)
            {
                var callStateOpt = stepContext.Options as TelExchangeState;
                if (callStateOpt != null)
                {
                    await TelExchangeStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await TelExchangeStateAccessor.SetAsync(stepContext.Context, new TelExchangeState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> PromptForRecipientNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchedRecipient = await TelExchangeStateAccessor.GetAsync(stepContext.Context);

            if (string.IsNullOrEmpty(searchedRecipient.RecipientFullName))
            {
                return await stepContext.PromptAsync(RecipientNamePrompt, new PromptOptions { Prompt = MessageFactory.Text(Properties.strings.querySearchPerson) }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchRecipientNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchedRecipient = await TelExchangeStateAccessor.GetAsync(stepContext.Context);
            
            if (string.IsNullOrEmpty(searchedRecipient.RecipientFullName))
            {
                searchedRecipient.RecipientFullName = (string)stepContext.Result;
            }

            var phoneNumber = await SearchNumberAsync();

            if (string.IsNullOrEmpty(phoneNumber))
            {
                var promptMessage = $"{Properties.strings.phoneNumberOf_1} {searchedRecipient.RecipientFullName} {Properties.strings.retryNumberSearchPrompt}";
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
                };
                return await stepContext.PromptAsync(RetryNumberSearchPrompt, promptOptions, cancellationToken);
            }

            searchedRecipient.PhoneNumber = phoneNumber;
            
            await TelExchangeStateAccessor.SetAsync(stepContext.Context, searchedRecipient);

            return await stepContext.NextAsync();
        }

        private async Task<string> SearchNumberAsync()
        {
            var phoneNumber = "555-2368 (Ghost Busters!)"; // TODO remove hardcoded number

            // TODO INSERT QUERY TO DATABASE HERE
            
            return phoneNumber;
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchedRecipient = await TelExchangeStateAccessor.GetAsync(stepContext.Context);
            if (string.IsNullOrEmpty(searchedRecipient.PhoneNumber)) 
            {
                var retry = (bool) stepContext.Result;
                if(retry)
                {
                    await ResetState(stepContext);
                    return await stepContext.
                }
                else
                {
                    return await stepContext.EndDialogAsync();
                }
            }

            if (searchedRecipient.IntentName == Intents.TelephoneExchange)
            {
                var textMessage = $"{Properties.strings.phoneNumberOf_1} {searchedRecipient.RecipientFullName} {Properties.strings.phoneNumberOf_2} " + phoneNumber + ".";

                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                        .Text(textMessage, textMessage, InputHints.IgnoringInput)
                        , cancellationToken
                );

                var forwardPromptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(Properties.strings.forwardCallPrompt),
                    RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
                };
                return await stepContext.PromptAsync(ConfirmForwardingPrompt, forwardPromptOptions, cancellationToken);
            }

            return await stepContext.NextAsync();
        }

        private async Task ResetState(WaterfallStepContext stepContext)
        {
            var intent = (await TelExchangeStateAccessor.GetAsync(stepContext.Context)).IntentName;
            var state = new TelExchangeState();
            state.IntentName = intent;

            await TelExchangeStateAccessor.SetAsync(stepContext.Context, state);
        }

        private Task<DialogTurnResult> ForwardCallStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<DialogTurnResult> EndTelExchangeDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
