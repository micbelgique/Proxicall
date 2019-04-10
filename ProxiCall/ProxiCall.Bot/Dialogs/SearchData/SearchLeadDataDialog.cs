using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Models.Intents;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services.ProxiCallCRM;

namespace ProxiCall.Bot.Dialogs.SearchData
{
    public class SearchLeadDataDialog : ComponentDialog
    {
        private readonly StateAccessors _accessors;
        private readonly ILoggerFactory _loggerFactory;
        private readonly BotServices _botServices;
        private readonly IServiceProvider _serviceProvider;

        private readonly LeadService _leadService;

        private const string _searchLeadDataWaterfall = "searchLeadDataWaterfall";
        private const string _leadFullNamePrompt = "leadFullNamePrompt";
        private const string _retryFetchingMinimumDataFromUserPrompt = "retryFetchingMinimumDataFromUserPrompt";
        private const string _confirmForwardingPrompt = "confirmForwardingPrompt";

        public SearchLeadDataDialog(StateAccessors accessors, ILoggerFactory loggerFactory, BotServices botServices, IServiceProvider serviceProvider) : base(nameof(SearchLeadDataDialog))
        {
            _accessors = accessors;
            _loggerFactory = loggerFactory;
            _botServices = botServices;
            _serviceProvider = serviceProvider;

            _leadService = (LeadService)_serviceProvider.GetService(typeof(LeadService));

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskForLeadFullNameStepAsync,
                SearchLeadStepAsync,
                ResultHandlerStepAsync,
                EndSearchDialogStepAsync
            };

            AddDialog(new WaterfallDialog(_searchLeadDataWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(_leadFullNamePrompt));
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
            var currentUser = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => null);
            if (currentUser == null)
            {
                if (stepContext.Options is LoggedUserState callStateOpt)
                {
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, new LoggedUserState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskForLeadFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context);

            //Asking for the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Lead.FullName))
            {
                return await stepContext.PromptAsync(_leadFullNamePrompt, new PromptOptions {
                    Prompt = MessageFactory.Text(CulturedBot.AskSearchedPersonFullName) }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchLeadStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Lead.FullName))
            {
                crmState.Lead.FullName = (string)stepContext.Result;
            }

            //Searching the lead
            var fullNameGivenByUser = crmState.Lead.FullName;
            crmState.Lead = await SearchLeadAsync(stepContext.Context, crmState.Lead.FirstName, crmState.Lead.LastName);

            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Lead == null)
            {
                promptMessage = $"{string.Format(CulturedBot.NamedObjectNotFound, fullNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
            }
            else if (luisState.IntentName == Intents.MakeACall)
            {
                if(crmState.Lead.PhoneNumber == null)
                {
                    userState.WantsToCallButNumberNotFound=true;
                    promptMessage = $"{string.Format(CulturedBot.PhoneNumberNotFound, fullNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
                }
            }
            var needsRetry = !string.IsNullOrEmpty(promptMessage);
            if (needsRetry)
            {
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingMinimumDataFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        //Searching Lead in Database
        private async Task<Lead> SearchLeadAsync(ITurnContext turnContext, string firstName, string lastName)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(turnContext, () => new LoggedUserState());
            return await _leadService.GetLeadByName(userState.LoggedUser.Token, firstName, lastName);
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            //Handling when lead not found
            if (crmState.Lead == null || userState.WantsToCallButNumberNotFound)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetLead();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    return await stepContext.ReplaceDialogAsync(_searchLeadDataWaterfall, cancellationToken);
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
            if (luisState.IntentName == Intents.SearchLeadData || luisState.IntentName == Intents.SearchCompanyData)
            {
                var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
                var hasOnlyPhoneEntity =
                    !(luisState.Entities.Contains(LuisState.SEARCH_ADDRESS_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_COMPANY_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_EMAIL_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME));

                var wantPhoneOnly = wantPhone && hasOnlyPhoneEntity;
                var wantPhoneOfContact = wantPhoneOnly && luisState.IntentName == Intents.SearchCompanyData;

                userState.IsEligibleForPotentialForwarding = (wantPhoneOnly || wantPhoneOfContact) && !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);

                //Creating adapted response
                var textMessage = await FormatMessageWithWantedData(stepContext);

                //Sending response
                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                        .Text(textMessage, textMessage, InputHints.IgnoringInput)
                        , cancellationToken
                );

                //Asking if user wants to forward the call
                if (userState.IsEligibleForPotentialForwarding)
                {
                    var forwardPromptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(CulturedBot.AskIfWantForwardCall),
                        RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                    };
                    return await stepContext.PromptAsync(_confirmForwardingPrompt, forwardPromptOptions, cancellationToken);
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<string> FormatMessageWithWantedData(WaterfallStepContext stepContext)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
            var wantAddress = luisState.Entities.Contains(LuisState.SEARCH_ADDRESS_ENTITYNAME);
            var wantCompany = luisState.Entities.Contains(LuisState.SEARCH_COMPANY_ENTITYNAME);
            var wantEmail = luisState.Entities.Contains(LuisState.SEARCH_EMAIL_ENTITYNAME);
            var wantContact = luisState.Entities.Contains(LuisState.SEARCH_CONTACT_ENTITYNAME);
            var wantContactName = luisState.Entities.Contains(LuisState.SEARCH_CONTACT_NAME_ENTITYNAME);
            var wantOppornunities = luisState.Entities.Contains(LuisState.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
            var wantNumberOppornunities = luisState.Entities.Contains(LuisState.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);

            var hasPhone = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
            var hasAddress = !string.IsNullOrEmpty(crmState.Lead.Address);
            var hasCompany = crmState.Lead.Company!= null && !string.IsNullOrEmpty(crmState.Lead.Company.Name);
            var hasEmail = !string.IsNullOrEmpty(crmState.Lead.Email);
            var hasOppornunities = false;

            if (wantOppornunities || wantNumberOppornunities)
            {
                //Searching opportunities with this lead
                //TODO : take off hardcode
                crmState.Opportunities = (List<OpportunityDetailed>) await SearchOpportunitiesAsync
                    (stepContext, crmState.Lead.FirstName, crmState.Lead.LastName, userState.LoggedUser.PhoneNumber);
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                hasOppornunities = crmState.Opportunities != null && crmState.Opportunities.Count != 0;
            }

            var wantedData = new StringBuilder(string.Empty);

            //Contact
            if (wantContact && hasCompany)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GiveContactName, crmState.Lead.Company.Name, crmState.Lead.FullName)}");
            }

            //Company
            if (wantCompany && hasCompany)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GiveCompanyName, crmState.Lead.FullName, crmState.Lead.Company.Name)}");
            }

            //Address
            if (wantAddress && hasAddress)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GiveHomeAddress, crmState.Lead.Address)}");
            }

            //Phone Number
            if (wantPhone && hasPhone)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GivePhoneNumber, crmState.Lead.PhoneNumber)}");
            }

            //Email
            if (wantEmail && hasEmail)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GiveEmailAddress, crmState.Lead.Email)}");
            }

            //Number of Opportunities
            if (wantNumberOppornunities || wantOppornunities)
            {
                var numberOfOpportunities = (crmState.Opportunities!=null? crmState.Opportunities.Count : 0);
                wantedData.AppendLine($"{string.Format(CulturedBot.GivenNumberOfOpportunities, numberOfOpportunities)}");
            }

            //Opportunities
            if (wantOppornunities && hasOppornunities)
            {
                var numberOfOpportunities = crmState.Opportunities.Count;
                for (int i = 0; i < crmState.Opportunities.Count; i++)
                {
                    wantedData.Append(string.Format(CulturedBot.ListOpportunities,
                        crmState.Opportunities[i].Product.Title, crmState.Opportunities[i].CreationDate?.ToShortDateString()));
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

            var hasNoResults = !(hasCompany || hasAddress || hasPhone || hasEmail || hasOppornunities);
            if (hasNoResults)
            {
                var hasMoreThanOneWantedInfos = luisState.Entities.Count > 1;
                if (hasMoreThanOneWantedInfos && !wantOppornunities)
                {
                    wantedData.Append($"{CulturedBot.NoDataFoundInDB}.");
                }
                else if(!wantOppornunities)
                {
                    wantedData.Append($"{CulturedBot.ThisDataNotFoundInDB}");
                }
            }
            return $"{wantedData.ToString()}";
        }
        
        //Searching Opportunities in Database
        private async Task<IEnumerable<OpportunityDetailed>> SearchOpportunitiesAsync(WaterfallStepContext stepContext, string leadFirstName, string leadLastName, string ownerPhoneNumber)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());
            var opportunities = await _leadService.GetOpportunities(userState.LoggedUser.Token, leadFirstName, leadLastName, ownerPhoneNumber);
            return opportunities;
        }

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            var isSearchLeadData =
                luisState.IntentName == Intents.SearchLeadData
                ||
                luisState.IntentName == Intents.SearchCompanyData && luisState.Entities.Contains(LuisState.SEARCH_CONTACT_ENTITYNAME);
            
            var forward = false;

            if (isSearchLeadData)
            {
                if(userState.IsEligibleForPotentialForwarding)
                {
                    forward = (bool)stepContext.Result;
                }
                if (!forward)
                {
                    //Ending Dialog
                    var message = CulturedBot.AskForRequest;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );

                    userState.IsEligibleForPotentialForwarding = false;
                    crmState.ResetLead();
                    luisState.ResetAll();
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                }
            }

            var isMakeACall = luisState.IntentName == Intents.MakeACall;
            var hasPhoneNumber = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);

            if (forward || (isMakeACall && hasPhoneNumber))
            {
                //"Forwarding" the call
                var textMessage = CulturedBot.InformAboutForwardingCall;
                Activity activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
                var entity = new Entity();
                entity.Properties.Add("forward", JToken.Parse(crmState.Lead.PhoneNumber));
                activity.Entities.Add(entity);

                await stepContext.Context.SendActivityAsync(activity, cancellationToken);

                userState.IsEligibleForPotentialForwarding = false;
                crmState.ResetLead();
                luisState.ResetAll();
                luisState.ResetIntentIfNoEntities();
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
            }
            return await stepContext.EndDialogAsync();
        }
    }
}
