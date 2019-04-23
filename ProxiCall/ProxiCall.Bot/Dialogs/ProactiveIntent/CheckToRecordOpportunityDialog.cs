using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxiCall.Bot.Dialogs.CreateData;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Resources;

namespace ProxiCall.Bot.Dialogs.ProactiveIntent
{
    public class CheckToRecordOpportunityDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StateAccessors _accessors;

        private const string _checkToRecordOpportunityDataWaterfall = "checkToRecordOpportunityDataWaterfall";
        private const string _askIfSomethingToRecordPrompt = "askIfSomethingToRecordPrompt";
        private const string _handleAnswerPrompt = "handleAnswerPrompt";

        public CheckToRecordOpportunityDialog(StateAccessors accessors, ILoggerFactory loggerFactory,
            BotServices botServices, IServiceProvider serviceProvider) : base(nameof(CheckToRecordOpportunityDialog))
        {
            _accessors = accessors;
            _loggerFactory = loggerFactory;
            _botServices = botServices;
            _serviceProvider = serviceProvider;

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskIfWantToRecordStepAsync,
                ResultHandlerStepAsync,
            };
            AddDialog(new WaterfallDialog(_checkToRecordOpportunityDataWaterfall, waterfallSteps));
            AddDialog(new ConfirmPrompt(_askIfSomethingToRecordPrompt, defaultLocale: "fr-fr"));
            AddDialog(new TextPrompt(_handleAnswerPrompt));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initializing CurrentUserAccessor
            var currentUser = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            if (currentUser == null)
            {
                if (stepContext.Options is LoggedUserState callStateOpt)
                {
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, callStateOpt, cancellationToken);
                }
                else
                {
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, new LoggedUserState(), cancellationToken);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> AskIfWantToRecordStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Avez-vous quelque chose à encoder?"),
                RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
            };
            return await stepContext.PromptAsync(_askIfSomethingToRecordPrompt, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            var wantsToCreateOpportunity = (bool)stepContext.Result;

            if(wantsToCreateOpportunity)
            {
                userState.WantsToEndCall = false;
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                AddDialog(ActivatorUtilities.CreateInstance<CreateOpportunityDialog>(_serviceProvider));
                return await stepContext.ReplaceDialogAsync(nameof(CreateOpportunityDialog), cancellationToken: cancellationToken);
            }

            //"Ending" the call
            var textMessage = "Très bien aurevoir!";
            Activity activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
            var entity = new Entity();
            entity.Properties.Add("endcall", null);
            activity.Entities.Add(entity);

            await stepContext.Context.SendActivityAsync(activity, cancellationToken);
            userState.WantsToEndCall = true;
            await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
