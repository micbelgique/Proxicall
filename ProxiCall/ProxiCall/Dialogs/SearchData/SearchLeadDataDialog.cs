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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.SearchData
{
    public class SearchLeadDataDialog : ComponentDialog
    {
        public IStatePropertyAccessor<CRMState> CRMStateAccessor { get; }
        public IStatePropertyAccessor<LuisState> LuisStateAccessor { get; }
        public ILoggerFactory LoggerFactory { get; }
        public BotServices BotServices { get; }

        private const string _searchLeadDataWaterfall = "searchLeadDataWaterfall";
        private const string _leadFullNamePrompt = "leadFullNamePrompt";
        private const string _retryFetchingMinimumDataFromUserPrompt = "retryFetchingMinimumDataFromUserPrompt";
        private const string _confirmForwardingPrompt = "confirmForwardingPrompt";
        private LoginDTO _currentUser;

        public SearchLeadDataDialog(IStatePropertyAccessor<CRMState> crmStateAccessor, IStatePropertyAccessor<LuisState> luisStateAccessor, LoginDTO currentUser,
            ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(SearchLeadDataDialog))
        {
            CRMStateAccessor = crmStateAccessor;
            LuisStateAccessor = luisStateAccessor;
            _currentUser = currentUser;
            LoggerFactory = loggerFactory;
            BotServices = botServices;

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

        private async Task<DialogTurnResult> AskForLeadFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);

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
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Lead.FullName))
            {
                crmState.Lead.FullName = (string)stepContext.Result;
            }

            //Searching the lead
            var fullNameGivenByUser = crmState.Lead.FullName;
            crmState.Lead = await SearchLeadAsync(crmState.Lead.FirstName, crmState.Lead.LastName);

            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Lead == null)
            {
                promptMessage = $"{string.Format(CulturedBot.LeadNotFound, fullNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
            }
            else if (luisState.IntentName == Intents.MakeACall)
            {
                if(crmState.Lead.PhoneNumber == null)
                {
                    crmState.WantsToCallButNumberNotFound=true;
                    promptMessage = $"{string.Format(CulturedBot.PhoneNumberNotFound, fullNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
                }
            }
            var needsRetry = !string.IsNullOrEmpty(promptMessage);
            if (needsRetry)
            {
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

        //Searching Lead in Database
        private async Task<Lead> SearchLeadAsync(string firstName, string lastName)
        {
            var leadService = new LeadService(_currentUser.Token);
            return await leadService.GetLeadByName(firstName, lastName);
        }

        private async Task<DialogTurnResult> ResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            //Handling when lead not found
            if (crmState.Lead == null || crmState.WantsToCallButNumberNotFound)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetLead();
                    await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
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
                    await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Giving informations to User
            if (luisState.IntentName == Intents.SearchLeadData || luisState.IntentName == Intents.SearchCompanyData)
            {
                var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
                var hasOnlyOneEntity =
                    !(luisState.Entities.Contains(LuisState.SEARCH_ADDRESS_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_COMPANY_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_EMAIL_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME));
                var wantPhoneOnly = wantPhone && hasOnlyOneEntity;
                var wantPhoneOfContact = wantPhoneOnly && luisState.IntentName == Intents.SearchCompanyData;

                //Creating adapted response
                var textMessage = await FormatMessageWithWantedData(stepContext);

                //Sending response
                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                        .Text(textMessage, textMessage, InputHints.IgnoringInput)
                        , cancellationToken
                );

                //Asking if user wants to forward the call
                if ((wantPhoneOnly || wantPhoneOfContact) && !string.IsNullOrEmpty(crmState.Lead.PhoneNumber))
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
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
            var wantAddress = luisState.Entities.Contains(LuisState.SEARCH_ADDRESS_ENTITYNAME);
            var wantCompany = luisState.Entities.Contains(LuisState.SEARCH_COMPANY_ENTITYNAME);
            var wantEmail = luisState.Entities.Contains(LuisState.SEARCH_EMAIL_ENTITYNAME);

            var hasPhone = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
            var hasCompany = crmState.Lead.Company!= null && !string.IsNullOrEmpty(crmState.Lead.Company.Name);
            var hasEmail = !string.IsNullOrEmpty(crmState.Lead.Email);
            var hasAddress = !string.IsNullOrEmpty(crmState.Lead.Address);

            var wantedData = new StringBuilder(string.Empty);
            var missingWantedData = string.Empty;

            var hasMoreThanOneWantedInfo = luisState.Entities.Count > 1;
            var hasOneOrMoreResult =
                (wantCompany && hasCompany) || (wantAddress && hasAddress)
                || (wantPhone && hasPhone) || (wantEmail && hasEmail);

            if (hasOneOrMoreResult)
            {
                if (luisState.IntentName != Intents.SearchCompanyData)
                {
                    wantedData.AppendLine($"{string.Format(CulturedBot.IntroduceLeadData, crmState.Lead.FullName)}");
                }

                if (wantCompany)
                {
                    if (!hasCompany)
                    {
                        missingWantedData += $"{CulturedBot.SayCompany}. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"{CulturedBot.SayCompany} : {crmState.Lead.Company}.");
                    }
                }

                if (wantAddress)
                {
                    if (!hasAddress)
                    {
                        missingWantedData += $"{CulturedBot.SayHomeAddress}. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"{CulturedBot.SayHomeAddress} : {crmState.Lead.Address}.");
                    }
                }

                if (wantPhone)
                {
                    if (!hasPhone)
                    {
                        missingWantedData += $"{CulturedBot.SayPhoneNumber}. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"{CulturedBot.SayPhoneNumber} : {crmState.Lead.PhoneNumber}.");
                    }
                }

                if (wantEmail)
                {
                    if (!hasEmail)
                    {
                        missingWantedData += $"{CulturedBot.SayEmailAddress}. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"{CulturedBot.SayEmailAddress} : {crmState.Lead.Email}.");
                    }
                }
            }
            else
            {
                if(hasMoreThanOneWantedInfo)
                {
                    wantedData.Append($"{CulturedBot.NoDataFoundInDB}.");
                }
                else
                {
                    wantedData.Append($"{CulturedBot.ThisDataNotFoundInDB}");
                }
            }
            
            if(!string.IsNullOrEmpty(missingWantedData))
            {
                missingWantedData = $"{string.Format(CulturedBot.MissingDataForThisLead, missingWantedData)}";
            }
            return $"{wantedData.ToString()} {missingWantedData}";
        }

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            var isSearchLeadData =
                luisState.IntentName == Intents.SearchLeadData
                ||
                luisState.IntentName == Intents.SearchCompanyData && luisState.Entities.Contains(LuisState.SEARCH_CONTACT_ENTITYNAME);
            var isMakeACall = luisState.IntentName == Intents.MakeACall;
            var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
            var hasPhoneNumber = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
            var hasOnlyOneEntity =
                    !(luisState.Entities.Contains(LuisState.SEARCH_ADDRESS_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_COMPANY_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_EMAIL_ENTITYNAME)
                    ||
                    luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME));
            var wantPhoneOnly = wantPhone && hasOnlyOneEntity;
            var wantPhoneOfContact = wantPhoneOnly && luisState.IntentName == Intents.SearchCompanyData;
            var forward = false;

            if (isSearchLeadData)
            {
                if(wantPhoneOnly || wantPhoneOfContact)
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

                    crmState.ResetLead();
                    luisState.ResetAll();
                    await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                }
            }
            if (forward || (isMakeACall && hasPhoneNumber))
            {
                //"Forwarding" the call
                var textMessage = CulturedBot.InformAboutForwardingCall;
                Activity activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
                var entity = new Entity();
                entity.Properties.Add("forward", JToken.Parse(crmState.Lead.PhoneNumber));
                activity.Entities.Add(entity);

                await stepContext.Context.SendActivityAsync(activity, cancellationToken);

                crmState.ResetLead();
                luisState.ResetAll();
                luisState.ResetIntentIfNoEntities();
                await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
            }
            return await stepContext.EndDialogAsync();
        }
    }
}
