using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using ProxiCall.Dialogs.Shared;
using ProxiCall.Models;
using ProxiCall.Resources;
using ProxiCall.Services.ProxiCallCRM;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.SearchData
{
    public class SearchCompanyDataDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StateAccessors _accessors;
        private const string _searchCompanyDataWaterfall = "searchCompanyDataWaterfall";
        private const string _companyNamePrompt = "companyFullNamePrompt";
        private const string _retryFetchingMinimumDataFromUserPrompt = "retryFetchingMinimumDataFromUserPrompt";
        private const string _confirmForwardingPrompt = "confirmForwardingPrompt";


        public SearchCompanyDataDialog(StateAccessors accessors, ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(SearchCompanyDataDialog))
        {
            _accessors = accessors;
            _loggerFactory = loggerFactory;
            _botServices = botServices;

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskForCompanyFullNameStepAsync,
                SearchCompanyStepAsync,
                ResultHandlerStepAsync,
                EndSearchDialogStepAsync
            };

            AddDialog(new WaterfallDialog(_searchCompanyDataWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(_companyNamePrompt));
            AddDialog(new ConfirmPrompt(_retryFetchingMinimumDataFromUserPrompt, defaultLocale: "fr-fr"));
            AddDialog(new ConfirmPrompt(_confirmForwardingPrompt, defaultLocale: "fr-fr"));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initializing CRMStateAccessor
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => null);
            if (crmState == null)
            {
                if (stepContext.Options is CRMState callStateOpt)
                {
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, new CRMState());
                }
            }

            //Initializing LuisStateAccessor
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => null);
            if (luisState == null)
            {
                if (stepContext.Options is LuisState callStateOpt)
                {
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, new LuisState());
                }
            }

            //Initializing CurrentUserAccessor
            var currentUser = await _accessors.UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (currentUser == null)
            {
                if (stepContext.Options is UserProfile callStateOpt)
                {
                    await _accessors.UserProfileAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await _accessors.UserProfileAccessor.SetAsync(stepContext.Context, new UserProfile());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskForCompanyFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());

            //Asking for the name of the company if not already given
            if (string.IsNullOrEmpty(crmState.Company.Name))
            {
                return await stepContext.PromptAsync(_companyNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Quelle est le nom de la compagnie concernée pour cette demande?")
                }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchCompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Company.Name))
            {
                crmState.Company.Name = (string)stepContext.Result;
            }

            //Searching the compan
            var companyNameGivenByUser = crmState.Company.Name;
            crmState.Company = await SearchCompanyAsync(stepContext.Context, crmState.Company.Name);

            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Company == null || string.IsNullOrEmpty(crmState.Company.Name))
            {
                promptMessage = $"{string.Format(CulturedBot.LeadNotFound, companyNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingMinimumDataFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        //Searching Company in Database
        private async Task<Company> SearchCompanyAsync(ITurnContext turnContext, string name)
        {
            var user = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            var companyService = new CompanyService(user.Token);
            return await companyService.GetCompanyByName(name);
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Handling when company not found
            if (crmState.Company == null || string.IsNullOrEmpty(crmState.Company.Name))
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetLead();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    return await stepContext.ReplaceDialogAsync(_searchCompanyDataWaterfall, cancellationToken);
                }
                else
                {
                    //Ending Dialog if user decides not to retry
                    var message = CulturedBot.AskForRequest;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );

                    crmState.ResetLead();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Giving informations to User
            //Creating adapted response
            var textMessage = await FormatMessageWithWantedData(stepContext);

            //Sending response
            await stepContext.Context
                .SendActivityAsync(MessageFactory
                    .Text(textMessage, textMessage, InputHints.IgnoringInput)
                    , cancellationToken
            );

            if(luisState.Entities.Contains(LuisState.SEARCH_CONTACT_ENTITYNAME))
            {
                AddDialog(new SearchLeadDataDialog(_accessors, _loggerFactory, _botServices));
                crmState.Lead = crmState.Company.Contact;
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                return await stepContext.ReplaceDialogAsync(nameof(SearchLeadDataDialog));
            }
            return await stepContext.NextAsync();
        }
        
        private async Task<string> FormatMessageWithWantedData(WaterfallStepContext stepContext)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            var wantLead = luisState.Entities.Contains(LuisState.SEARCH_CONTACT_ENTITYNAME);
            var wantLeadName = luisState.Entities.Contains(LuisState.SEARCH_CONTACT_NAME_ENTITYNAME);
            var wantOppornunities = false;

            var hasLeadName = crmState.Company.Contact != null && !string.IsNullOrEmpty(crmState.Company.Contact.FullName);
            var hasOppornunities = false;

            var wantedData = new StringBuilder(string.Empty);
            var missingWantedData = string.Empty;

            var hasMoreThanOneWantedInfo = luisState.Entities.Count > 1;
            var hasOneOrMoreResult =
                (wantOppornunities && hasOppornunities) || (wantLead && hasLeadName);

            if (hasOneOrMoreResult)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.IntroduceLeadData, crmState.Company.Name)}");
                
                if (wantLead)
                {
                    if (!hasLeadName)
                    {
                        missingWantedData += $"Nom du lead. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"Nom du lead : {crmState.Company.Contact.FullName}.");
                    }
                }

                if (wantOppornunities)
                {
                    if (!hasOppornunities)
                    {
                        missingWantedData += $"{CulturedBot.SayHomeAddress}. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"{CulturedBot.SayHomeAddress} : {crmState.Lead.Address}.");
                    }
                }
            }
            else
            {
                if (hasMoreThanOneWantedInfo)
                {
                    wantedData.Append($"{CulturedBot.NoDataFoundInDB}.");
                }
                else
                {
                    wantedData.Append($"{CulturedBot.ThisDataNotFoundInDB}");
                }
            }

            if (!string.IsNullOrEmpty(missingWantedData))
            {
                missingWantedData = $"{string.Format(CulturedBot.MissingDataForThisLead, missingWantedData)}";
            }
            return $"{wantedData.ToString()} {missingWantedData}";
        }

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            var message = CulturedBot.AskForRequest;
            await stepContext.Context.SendActivityAsync(MessageFactory
                .Text(message, message, InputHints.AcceptingInput)
                , cancellationToken
            );

            crmState.ResetLead();
            luisState.ResetAll();
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
            return await stepContext.EndDialogAsync();
        }
    }
}
