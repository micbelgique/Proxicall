using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services.ProxiCallCRM;
using ProxiCall.Library.ProxiCallLuis;

namespace ProxiCall.Bot.Dialogs.SearchData
{
    public class SearchCompanyDataDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StateAccessors _accessors;
        private readonly CompanyService _companyService;

        private const string _searchCompanyDataWaterfall = "searchCompanyDataWaterfall";
        private const string _companyNamePrompt = "companyFullNamePrompt";
        private const string _retryFetchingMinimumDataFromUserPrompt = "retryFetchingMinimumDataFromUserPrompt";
        private const string _confirmForwardingPrompt = "confirmForwardingPrompt";


        public SearchCompanyDataDialog(StateAccessors accessors, ILoggerFactory loggerFactory, BotServices botServices, IServiceProvider serviceProvider) : base(nameof(SearchCompanyDataDialog))
        {
            _accessors = accessors;
            _loggerFactory = loggerFactory;
            _botServices = botServices;
            _serviceProvider = serviceProvider;

            _companyService = (CompanyService)_serviceProvider.GetService(typeof(CompanyService));

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskForCompanyFullNameStepAsync,
                SearchCompanyStepAsync,
                ResultHandlerStepAsync,
                EndSearchDialogStepAsync
            };
            
            var culture = CulturedBot.Culture?.Name;
            AddDialog(new WaterfallDialog(_searchCompanyDataWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(_companyNamePrompt));
            AddDialog(new ConfirmPrompt(_retryFetchingMinimumDataFromUserPrompt, defaultLocale: culture));
            AddDialog(new ConfirmPrompt(_confirmForwardingPrompt, defaultLocale: culture));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initializing CRMStateAccessor
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            if (crmState == null)
            {
                if (stepContext.Options is CRMState callStateOpt)
                {
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, callStateOpt, cancellationToken);
                }
                else
                {
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, new CRMState(), cancellationToken);
                }
            }

            //Initializing LuisStateAccessor
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            if (luisState == null)
            {
                if (stepContext.Options is LuisState callStateOpt)
                {
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, callStateOpt, cancellationToken);
                }
                else
                {
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, new LuisState(), cancellationToken);
                }
            }

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

        private async Task<DialogTurnResult> AskForCompanyFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);

            //Asking for the name of the company if not already given
            if (string.IsNullOrEmpty(crmState.Company.Name))
            {
                var activityPrompt = MessageFactory.Text(CulturedBot.AskCompanyName);
                activityPrompt.Locale = CulturedBot.Culture?.Name;

                return await stepContext.PromptAsync(_companyNamePrompt, new PromptOptions
                {
                    Prompt = activityPrompt
                }, cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SearchCompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);

            //Gathering the name of the company if not already given
            if (string.IsNullOrEmpty(crmState.Company.Name))
            {
                crmState.Company.Name = (string)stepContext.Result;
            }

            //Searching for the company in the database
            var companyNameGivenByUser = crmState.Company.Name;
            crmState.Company = await SearchCompanyAsync(stepContext.Context, crmState.Company.Name);

            //Asking for retry if necessary
            if (crmState.Company == null || string.IsNullOrEmpty(crmState.Company.Name))
            {
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                var promptMessage = $"{string.Format(CulturedBot.NamedObjectNotFound, companyNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
                
                var activityPrompt = MessageFactory.Text(promptMessage);
                activityPrompt.Locale = CulturedBot.Culture?.Name;

                var activityRetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo);
                activityRetryPrompt.Locale = CulturedBot.Culture?.Name;

                var promptOptions = new PromptOptions
                {
                    Prompt = activityPrompt,
                    RetryPrompt = activityRetryPrompt,
                };
                return await stepContext.PromptAsync(_retryFetchingMinimumDataFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<Company> SearchCompanyAsync(ITurnContext turnContext, string name)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(turnContext, () => new LoggedUserState());
            return await _companyService.GetCompanyByName(userState.LoggedUser.Token, name);
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);

            //Handling when company not found
            if (crmState.Company == null || string.IsNullOrEmpty(crmState.Company.Name))
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetCompany();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(_searchCompanyDataWaterfall, cancellationToken, cancellationToken);
                }
                else
                {
                    //Ending Dialog if user decides not to retry
                    var message = CulturedBot.AskForRequest;
                    var activity = MessageFactory.Text(message, message, InputHints.AcceptingInput);
                    activity.Locale = CulturedBot.Culture?.Name;
                    await stepContext.Context.SendActivityAsync(activity, cancellationToken);

                    crmState.ResetCompany();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            
            var wantOppornunities = luisState.Entities.Contains(ProxiCallEntities.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
            var wantNumberOppornunities = luisState.Entities.Contains(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);

            //Searching for lead contact
            //Redirect to SearchLeadDataDialog
            if (luisState.Entities.Contains(ProxiCallEntities.SEARCH_CONTACT_ENTITYNAME))
            {
                AddDialog(ActivatorUtilities.CreateInstance<SearchLeadDataDialog>(_serviceProvider));
                crmState.Lead = crmState.Company.Contact;
                if(crmState.Lead.Company==null)
                {
                    crmState.Lead = Lead.CloneWithCompany(crmState.Lead,crmState.Company);
                }
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(SearchLeadDataDialog), cancellationToken: cancellationToken);
            }
            else if (wantOppornunities || wantNumberOppornunities)
            {
                //Searching for Opportunities
                //Creating adapted response
                var textMessage = await FormatMessageWithOpportunities(stepContext);

                //Sending response
                var activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
                activity.Locale = CulturedBot.Culture?.Name;
                await stepContext.Context.SendActivityAsync(activity, cancellationToken);
                
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }


        private async Task<string> FormatMessageWithOpportunities(WaterfallStepContext stepContext)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            var wantOppornunities = luisState.Entities.Contains(ProxiCallEntities.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
            var wantNumberOppornunities = luisState.Entities.Contains(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);

            var hasOppornunities = false;

            if (wantOppornunities || wantNumberOppornunities)
            {
                //Searching opportunities with this lead
                crmState.Opportunities = (List<OpportunityDetailed>)await SearchOpportunitiesAsync
                    (stepContext, crmState.Company.Name, "32491180031");
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                hasOppornunities = crmState.Opportunities != null && crmState.Opportunities.Count != 0;
            }

            var wantedData = new StringBuilder(string.Empty);

            //Number of Opportunities
            if (wantNumberOppornunities || wantOppornunities)
            {
                var numberOfOpportunities = crmState.Opportunities?.Count ?? 0;
                wantedData.AppendLine($"{string.Format(CulturedBot.GivenNumberOfOpportunitiesByCompany, numberOfOpportunities)}");
            }

            //Opportunities
            if (wantOppornunities && hasOppornunities)
            {
                var numberOfOpportunities = crmState.Opportunities.Count;
                for (int i = 0; i < crmState.Opportunities.Count; i++)
                {
                    wantedData.Append(string.Format(CulturedBot.ListOpportunitiesOfCompany,
                        crmState.Opportunities[i].Lead.FullName, crmState.Opportunities[i].Product.Title, crmState.Opportunities[i].CreationDate?.ToString("dd MMMM", CulturedBot.Culture)));
                    if (i == (numberOfOpportunities - 2))
                    {
                        wantedData.Append($" {CulturedBot.LinkWithAnd} ");
                    }
                    else if (i != (numberOfOpportunities - 1))
                    {
                        wantedData.Append($", ");
                    }
                }
            }
            return $"{wantedData.ToString()}";
        }

        //Searching Opportunities in Database
        private async Task<IEnumerable<OpportunityDetailed>> SearchOpportunitiesAsync
            (WaterfallStepContext stepContext, string companyName, string ownerPhoneNumber)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());
            var opportunities = await _companyService.GetOpportunities(userState.LoggedUser.Token, companyName, ownerPhoneNumber);
            return opportunities;
        }

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);

            var message = CulturedBot.AskForRequest;
            var activity = MessageFactory.Text(message, message, InputHints.AcceptingInput);
            activity.Locale = CulturedBot.Culture?.Name;
            await stepContext.Context.SendActivityAsync(activity, cancellationToken);

            crmState.ResetCompany();
            luisState.ResetAll();
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
            await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
