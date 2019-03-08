using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ProxiCall.Dialogs.Call
{
    public class CallDialog : ComponentDialog
    {
        //Recipient state for Call dialog
        private const string CallQueryDialog = "callQueryDialog";
        private const string RecipientPrompt = "recipientPrompt";
        private const string RecipientNumberPrompt = "recipientNumberPrompt";
        private const string ConfirmForwardingPrompt = "confirmForwardingPrompt";
        private const string EndCallDialogPrompt = "endCallDialogPrompt";

        public CallDialog(IStatePropertyAccessor<CallState> callStateAccessor, ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(CallDialog))
        {
            CallStateAccessor = callStateAccessor;
            LoggerFactory = loggerFactory;
            BotServices = botServices;

            var waterfallSteps = new WaterfallStep[]
            {
                //steps
                InitializeStateStepAsync,
                PromptForRecipientNameStepAsync,
                PromptForRecipientNumberStepAsync,
                ConfirmForwardingStepAsync,//Make a phone call if confirmed
                EndCallDialogStepsAsync,
            };
            AddDialog(new WaterfallDialog(CallQueryDialog, waterfallSteps));
            AddDialog(new TextPrompt(RecipientPrompt));
            AddDialog(new TextPrompt(RecipientNumberPrompt));
            AddDialog(new ConfirmPrompt(ConfirmForwardingPrompt, defaultLocale: "fr-fr"));//TODO change prompt type
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var callState = await CallStateAccessor.GetAsync(stepContext.Context, () => null);
            if (callState == null)
            {
                var callStateOpt = stepContext.Options as CallState;
                if (callStateOpt != null)
                {
                    await CallStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await CallStateAccessor.SetAsync(stepContext.Context, new CallState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> PromptForRecipientNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(RecipientPrompt, new PromptOptions { Prompt = MessageFactory.Text(Properties.strings.querySearchPerson) }, cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForRecipientNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var searchedRecipient = await CallStateAccessor.GetAsync(stepContext.Context);

            // Update the profile.
            searchedRecipient.RecipientFirstName = (string)stepContext.Result;

            var phoneNumber = "555-2368 (Ghost Busters!)"; // TODO remove hardcoded number

            // TODO INSERT QUERY TO DATABASE HERE

            var textMessage = $"{Properties.strings.phoneNumberOf_1} {stepContext.Result} {Properties.strings.phoneNumberOf_2} " + phoneNumber + ".";
            searchedRecipient.PhoneNumber = phoneNumber;

            await CallStateAccessor.SetAsync(stepContext.Context, searchedRecipient);

            await stepContext.Context
                .SendActivityAsync(MessageFactory
                    .Text(textMessage, textMessage, InputHints.IgnoringInput)
                    , cancellationToken);

            return await stepContext.ContinueDialogAsync();
        }

        private async Task<DialogTurnResult> ConfirmForwardingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = new PromptOptions
            {
                Prompt = MessageFactory.Text(Properties.strings.forwardCallPrompt),
                RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
            };
            return await stepContext.PromptAsync(ConfirmForwardingPrompt, response, cancellationToken);
        }

        private async Task<DialogTurnResult> EndCallDialogStepsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var confirmation = (bool) stepContext.Result;
            string msg = null;
            if (confirmation)
            {
                msg = $"{Properties.strings.callForwardingConfirmed}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg, msg, InputHints.IgnoringInput), cancellationToken);
                //TODO send number to twilio
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(Properties.strings.goodbye), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        public IStatePropertyAccessor<CallState> CallStateAccessor { get; }
        public ILoggerFactory LoggerFactory { get; }
        public BotServices BotServices { get; }
    }
}
