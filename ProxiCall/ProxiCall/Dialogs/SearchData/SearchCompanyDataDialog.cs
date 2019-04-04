using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProxiCall.Dialogs.Shared;
using ProxiCall.Models;
using ProxiCall.Models.Intents;
using ProxiCall.Resources;
using ProxiCall.Services.ProxiCallCRM;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.SearchData
{
    public class SearchCompanyDataDialog : ComponentDialog
    {
        public IStatePropertyAccessor<CRMState> CRMStateAccessor { get; }
        public IStatePropertyAccessor<LuisState> LuisStateAccessor { get; }
        public ILoggerFactory LoggerFactory { get; }
        public BotServices BotServices { get; }

        private const string _searchCompanyDataWaterfall = "searchCompanyDataWaterfall";
        private const string _companyNamePrompt = "companyFullNamePrompt";
        private const string _retryFetchingMinimumDataFromUserPrompt = "retryFetchingMinimumDataFromUserPrompt";
        private const string _confirmForwardingPrompt = "confirmForwardingPrompt";

        public SearchCompanyDataDialog(IStatePropertyAccessor<CRMState> crmStateAccessor, IStatePropertyAccessor<LuisState> luisStateAccessor,
            ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(SearchCompanyDataDialog))
        {
            CRMStateAccessor = crmStateAccessor;
            LuisStateAccessor = luisStateAccessor;
            LoggerFactory = loggerFactory;
            BotServices = botServices;

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
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context, () => null);
            if (crmState == null)
            {
                if (stepContext.Options is CRMState callStateOpt)
                {
                    await CRMStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await CRMStateAccessor.SetAsync(stepContext.Context, new CRMState());
                }
            }

            //Initializing LuisStateAccessor
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context, () => null);
            if (luisState == null)
            {
                if (stepContext.Options is LuisState callStateOpt)
                {
                    await LuisStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await LuisStateAccessor.SetAsync(stepContext.Context, new LuisState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskForCompanyFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);

            //Asking for the name of the company if not already given
            if (string.IsNullOrEmpty(crmState.Company.Name))
            {
                return await stepContext.PromptAsync(_companyNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.AskCompanyName)
                }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchCompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            //Gathering the name of the company if not already given
            if (string.IsNullOrEmpty(crmState.Company.Name))
            {
                crmState.Company.Name = (string)stepContext.Result;
            }

            //Searching for the company in the database
            var companyNameGivenByUser = crmState.Company.Name;
            crmState.Company = await SearchCompanyAsync(stepContext, crmState.Company.Name);

            //Asking for retry if necessary
            if (crmState.Company == null || string.IsNullOrEmpty(crmState.Company.Name))
            {
                var promptMessage = $"{string.Format(CulturedBot.NamedObjectNotFound, companyNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
                await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingMinimumDataFromUserPrompt, promptOptions, cancellationToken);
            }

            await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        //Searching the company in the database
        private async Task<Company> SearchCompanyAsync(WaterfallStepContext stepContext, string name)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var companyService = new CompanyService(crmState.AuthToken);
            return await companyService.GetCompanyByName(name);
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            //Handling when company not found
            if (crmState.Company == null || string.IsNullOrEmpty(crmState.Company.Name))
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetCompany();
                    await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
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

                    crmState.ResetCompany();
                    luisState.ResetAll();
                    await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }
            
            var wantOppornunities = luisState.Entities.Contains(LuisState.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
            var wantNumberOppornunities = luisState.Entities.Contains(LuisState.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);

            //Searching for lead contact
            //Redirect to SearchLeadDataDialog
            if (luisState.Entities.Contains(LuisState.SEARCH_CONTACT_ENTITYNAME))
            {
                AddDialog(new SearchLeadDataDialog(CRMStateAccessor, LuisStateAccessor, LoggerFactory, BotServices));
                crmState.Lead = crmState.Company.Contact;
                if(crmState.Lead.Company==null)
                {
                    //TODO : ask Renaud : Creating a clone to prevent looping ok?
                    crmState.Lead = Lead.CloneWithCompany(crmState.Lead,crmState.Company);
                }
                await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                return await stepContext.ReplaceDialogAsync(nameof(SearchLeadDataDialog));
            }
            else if (wantOppornunities || wantNumberOppornunities)
            {
                //Searching for Opportunities
                //Creating adapted response
                var textMessage = await FormatMessageWithOpportunities(stepContext);

                //Sending response
                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                        .Text(textMessage, textMessage, InputHints.IgnoringInput)
                        , cancellationToken
                );
            }

            return await stepContext.NextAsync();
        }


        private async Task<string> FormatMessageWithOpportunities(WaterfallStepContext stepContext)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            var wantOppornunities = luisState.Entities.Contains(LuisState.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
            var wantNumberOppornunities = luisState.Entities.Contains(LuisState.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);

            var hasOppornunities = false;

            if (wantOppornunities || wantNumberOppornunities)
            {
                //Searching opportunities with this lead
                crmState.Opportunities = (List<Opportunity>)await SearchOpportunitiesAsync
                    (stepContext, crmState.Company.Name, "32491180031");
                await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                hasOppornunities = crmState.Opportunities != null && crmState.Opportunities.Count != 0;
            }

            var wantedData = new StringBuilder(string.Empty);

            //Number of Opportunities
            if (wantNumberOppornunities || wantOppornunities)
            {
                var numberOfOpportunities = (crmState.Opportunities != null ? crmState.Opportunities.Count : 0);
                wantedData.AppendLine($"{string.Format(CulturedBot.GivenNumberOfOpportunitiesByCompany, numberOfOpportunities)}");
            }

            //Opportunities
            if (wantOppornunities && hasOppornunities)
            {
                var numberOfOpportunities = crmState.Opportunities.Count;
                for (int i = 0; i < crmState.Opportunities.Count; i++)
                {
                    wantedData.Append(string.Format(CulturedBot.ListOpportunitiesOfCompany,
                        crmState.Opportunities[i].Lead.FullName, crmState.Opportunities[i].Product.Title, crmState.Opportunities[i].CreationDate.ToShortDateString()));
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
        private async Task<IEnumerable<Opportunity>> SearchOpportunitiesAsync
            (WaterfallStepContext stepContext, string companyName, string ownerPhoneNumber)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var companyService = new CompanyService(crmState.AuthToken);
            var opportunities = await companyService.GetOpportunities(companyName, ownerPhoneNumber);
            return opportunities;
        }

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            var message = CulturedBot.AskForRequest;
            await stepContext.Context.SendActivityAsync(MessageFactory
                .Text(message, message, InputHints.AcceptingInput)
                , cancellationToken
            );

            crmState.ResetCompany();
            luisState.ResetAll();
            await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
            return await stepContext.EndDialogAsync();
        }
    }
}
