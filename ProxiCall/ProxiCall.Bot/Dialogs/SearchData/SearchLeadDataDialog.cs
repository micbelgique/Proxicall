﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services.ProxiCallCRM;
using ProxiCall.Library.Dictionnaries.Lead;
using ProxiCall.Library.ProxiCallLuis;

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
            
            var culture = CulturedBot.Culture?.Name;
            AddDialog(new WaterfallDialog(_searchLeadDataWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(_leadFullNamePrompt));
            AddDialog(new TextPrompt(_retryFetchingMinimumDataFromUserPrompt/*, defaultLocale: culture*/));
            AddDialog(new TextPrompt(_confirmForwardingPrompt/*, defaultLocale: culture*/));
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
                var activityPrompt = MessageFactory.Text(CulturedBot.AskSearchedPersonFullName);
                activityPrompt.Locale = CulturedBot.Culture?.Name;

                return await stepContext.PromptAsync(_leadFullNamePrompt, new PromptOptions {
                    Prompt =  activityPrompt
                }, cancellationToken);
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
            else if (luisState.IntentName == ProxiCallIntents.MakeACall)
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
                var retry = stepContext.Result.ToString().ToLower().Equals(CulturedBot.Yes) ? true: false ;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetLead();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(_searchLeadDataWaterfall, cancellationToken: cancellationToken);
                }
                else
                {
                    //Ending Dialog if user decides not to retry
                    var message = CulturedBot.AskForRequest;
                    var activity = MessageFactory.Text(message, message, InputHints.AcceptingInput);
                    activity.Locale = CulturedBot.Culture?.Name;
                    await stepContext.Context.SendActivityAsync(activity, cancellationToken);
                    
                    crmState.ResetLead();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Giving informations to User
            if (luisState.IntentName == ProxiCallIntents.SearchLeadData || luisState.IntentName == ProxiCallIntents.SearchCompanyData)
            {
                var wantPhone = luisState.Entities.Contains(ProxiCallEntities.SEARCH_PHONENUMBER_ENTITYNAME);
                var hasOnlyPhoneEntity =
                    !(luisState.Entities.Contains(ProxiCallEntities.SEARCH_ADDRESS_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(ProxiCallEntities.SEARCH_COMPANY_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(ProxiCallEntities.SEARCH_EMAIL_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(ProxiCallEntities.SEARCH_PHONENUMBER_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(ProxiCallEntities.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME));

                var wantPhoneOnly = wantPhone && hasOnlyPhoneEntity;
                var wantPhoneOfContact = wantPhoneOnly && luisState.IntentName == ProxiCallIntents.SearchCompanyData;

                userState.IsEligibleForPotentialForwarding = (wantPhoneOnly || wantPhoneOfContact) && !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);

                //Creating adapted response
                var textMessage = await FormatMessageWithWantedData(stepContext);

                //Sending response
                var activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
                activity.Locale = CulturedBot.Culture?.Name;
                await stepContext.Context.SendActivityAsync(activity, cancellationToken);

                //Asking if user wants to forward the call
                if (userState.IsEligibleForPotentialForwarding)
                {
                    var activityPrompt = MessageFactory.Text(CulturedBot.AskIfWantForwardCall);
                    activityPrompt.Locale = CulturedBot.Culture?.Name;

                    var activityRetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo);
                    activityRetryPrompt.Locale = CulturedBot.Culture?.Name;

                    var forwardPromptOptions = new PromptOptions
                    {
                        Prompt = activityPrompt,
                        RetryPrompt = activityRetryPrompt,
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

            var wantPhone = luisState.Entities.Contains(ProxiCallEntities.SEARCH_PHONENUMBER_ENTITYNAME);
            var wantAddress = luisState.Entities.Contains(ProxiCallEntities.SEARCH_ADDRESS_ENTITYNAME);
            var wantCompany = luisState.Entities.Contains(ProxiCallEntities.SEARCH_COMPANY_ENTITYNAME);
            var wantEmail = luisState.Entities.Contains(ProxiCallEntities.SEARCH_EMAIL_ENTITYNAME);
            var wantContact = luisState.Entities.Contains(ProxiCallEntities.SEARCH_CONTACT_ENTITYNAME);
            var wantContactName = luisState.Entities.Contains(ProxiCallEntities.SEARCH_CONTACT_NAME_ENTITYNAME);
            var wantOppornunities = luisState.Entities.Contains(ProxiCallEntities.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
            var wantNumberOppornunities = luisState.Entities.Contains(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);
            var wantAnyInfoAboutOpportunities = wantOppornunities || wantNumberOppornunities;
            var onlyWantAnyInfoAboutOpportunities =
                wantAnyInfoAboutOpportunities && !( wantPhone || wantAddress || wantCompany || wantEmail || wantContact || wantContactName );

            var hasPhone = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
            var hasAddress = !string.IsNullOrEmpty(crmState.Lead.Address);
            var hasCompany = crmState.Lead.Company!= null && !string.IsNullOrEmpty(crmState.Lead.Company.Name);
            var hasEmail = !string.IsNullOrEmpty(crmState.Lead.Email);
            var hasOppornunities = false;

            var chosenSubjectPronoun = string.Empty;
            var chosenObjectPronoun = string.Empty;
            var chosenPossessivePronoun = string.Empty;

            // TODO : to be improved
            var isMale = crmState.Lead.Gender == 1;
            var isFemale = crmState.Lead.Gender == 2;
            if (isMale)
            {
                chosenSubjectPronoun = $"{CulturedBot.SayHe}";
                chosenObjectPronoun = $"{CulturedBot.SayHim}";
                chosenPossessivePronoun = $"{CulturedBot.SayHisPossesive}";
            }
            else if (isFemale)
            {
                chosenSubjectPronoun = $"{CulturedBot.SayShe}";
                chosenObjectPronoun = $"{CulturedBot.SayHer}";
                chosenPossessivePronoun = $"{CulturedBot.SayHerPossesive}";
            }
            else
            {
                chosenSubjectPronoun = $"{CulturedBot.SayHe}";
                chosenObjectPronoun = $"{CulturedBot.SayHisPossesive}";
                chosenPossessivePronoun = $"{CulturedBot.SayHisPossesive}";
            }

            if (wantOppornunities || wantNumberOppornunities)
            {
                //Searching opportunities with this lead
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
                wantedData.AppendLine($"{string.Format(CulturedBot.GiveHomeAddress, chosenSubjectPronoun, crmState.Lead.Address)}");
            }

            //Phone Number
            if (wantPhone && hasPhone)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GivePhoneNumber, chosenPossessivePronoun, crmState.Lead.PhoneNumber)}");
            }

            //Email
            if (wantEmail && hasEmail)
            {
                wantedData.AppendLine($"{string.Format(CulturedBot.GiveEmailAddress, chosenPossessivePronoun, crmState.Lead.Email)}");
            }

            //Number of Opportunities
            if (wantNumberOppornunities || wantOppornunities)
            {
                var numberOfOpportunities = (crmState.Opportunities!=null? crmState.Opportunities.Count : 0);

                if(numberOfOpportunities>0 || onlyWantAnyInfoAboutOpportunities)
                {
                    wantedData.AppendLine($"{string.Format(CulturedBot.GivenNumberOfOpportunities, numberOfOpportunities, chosenObjectPronoun)}");
                }
            }

            //Opportunities
            if (wantOppornunities && hasOppornunities)
            {
                var numberOfOpportunities = crmState.Opportunities.Count;
                for (int i = 0; i < crmState.Opportunities.Count; i++)
                {
                    wantedData.Append(string.Format(CulturedBot.ListOpportunities,
                        crmState.Opportunities[i].Product.Title, crmState.Opportunities[i].CreationDate?.ToString("dd MMMM", CulturedBot.Culture)));
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

            var hasNoWantedResults =
                !(
                    (wantContact && hasCompany) || (hasCompany && wantCompany) || (hasAddress && wantAddress) ||
                    (hasPhone && wantPhone) || (hasEmail && wantEmail) || (hasOppornunities && wantOppornunities)
                );

            if (hasNoWantedResults)
            {
                var hasMoreThanOneWantedInfos = luisState.Entities.Count > 1;
                if(!wantAnyInfoAboutOpportunities || !onlyWantAnyInfoAboutOpportunities)
                {
                    if (hasMoreThanOneWantedInfos)
                    {
                        wantedData.Append($"{CulturedBot.NoDataFoundInDB}.");
                    }
                    else
                    {
                        wantedData.Append($"{CulturedBot.ThisDataNotFoundInDB}");
                    }
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
                luisState.IntentName == ProxiCallIntents.SearchLeadData
                ||
                luisState.IntentName == ProxiCallIntents.SearchCompanyData && luisState.Entities.Contains(ProxiCallEntities.SEARCH_CONTACT_ENTITYNAME);
            
            var forward = false;

            if (isSearchLeadData)
            {
                if(userState.IsEligibleForPotentialForwarding)
                {
                    forward = stepContext.Result.ToString().ToLower().Equals(CulturedBot.Yes) ? true : false;
                }
                if (!forward)
                {
                    //Ending Dialog
                    var message = CulturedBot.AskForRequest;
                    var activity = MessageFactory.Text(message, message, InputHints.AcceptingInput);
                    activity.Locale = CulturedBot.Culture?.Name;
                    await stepContext.Context.SendActivityAsync(activity, cancellationToken);

                    userState.IsEligibleForPotentialForwarding = false;
                    crmState.ResetLead();
                    luisState.ResetAll();
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                }
            }

            var isMakeACall = luisState.IntentName == ProxiCallIntents.MakeACall;
            var hasPhoneNumber = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);

            if (forward || (isMakeACall && hasPhoneNumber))
            {
                //"Forwarding" the call
                var textMessage = CulturedBot.InformAboutForwardingCall;
                Activity activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
                var entity = new Entity();
                entity.Properties.Add("forward", JToken.Parse(crmState.Lead.PhoneNumber));
                activity.Entities.Add(entity);
                activity.Locale = CulturedBot.Culture?.Name;

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
