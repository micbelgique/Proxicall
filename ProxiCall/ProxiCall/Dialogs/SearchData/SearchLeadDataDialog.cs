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

        public SearchLeadDataDialog(IStatePropertyAccessor<CRMState> crmStateAccessor, IStatePropertyAccessor<LuisState> luisStateAccessor,
            ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(SearchLeadDataDialog))
        {
            CRMStateAccessor = crmStateAccessor;
            LuisStateAccessor = luisStateAccessor;
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
                    Prompt = MessageFactory.Text(Localization.AskSearchedPersonFullName) }, cancellationToken);
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
            var fullName = crmState.Lead.FullName;
            crmState.Lead = await SearchLeadAsync(crmState.Lead.FirstName, crmState.Lead.LastName);

            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Lead == null)
            {
                promptMessage = $"{fullName} {Properties.strings.retryNumberSearchPrompt}";
            }
            else if (luisState.IntentName == Intents.MakeACall)
            {
                if(crmState.Lead.PhoneNumber == null)
                {
                    crmState.WantsToCallButNumberNotFound=true;
                    promptMessage = $"Le numéro de {crmState.Lead.FullName} {Properties.strings.retryNumberSearchPrompt}";
                }
            }
            var needsRetry = !string.IsNullOrEmpty(promptMessage);
            if (needsRetry)
            {
                await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
                };
                return await stepContext.PromptAsync(_retryFetchingMinimumDataFromUserPrompt, promptOptions, cancellationToken);
            }

            await CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        //Searching Lead in Database
        private async Task<Lead> SearchLeadAsync(string firstName, string lastName)
        {
            var leadService = new LeadService();
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
                    var message = Properties.strings.welcome_2;
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
            if (luisState.IntentName == Intents.SearchData)
            {
                var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
                var hasOnlyOneEntity = luisState.Entities.Count == 1;

                //Creating adapted response
                var textMessage = await FormatMessageWithWantedData(stepContext);

                //Sending response
                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                        .Text(textMessage, textMessage, InputHints.IgnoringInput)
                        , cancellationToken
                );

                var wantPhoneOnly = wantPhone && hasOnlyOneEntity;
                //Asking if user wants to forward the call
                if (wantPhoneOnly && !string.IsNullOrEmpty(crmState.Lead.PhoneNumber))
                {
                    var forwardPromptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(Properties.strings.forwardCallPrompt),
                        RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
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
            var hasCompany = !string.IsNullOrEmpty(crmState.Lead.Company.Name);
            var hasEmail = !string.IsNullOrEmpty(crmState.Lead.Email);
            var hasAddress = !string.IsNullOrEmpty(crmState.Lead.Address);

            var wantedData = new StringBuilder(string.Empty);
            var missingWantedData = string.Empty;

            var hasOneOrMoreResult =
                (wantCompany && hasCompany) || (wantAddress && hasAddress)
                || (wantPhone && hasPhone) || (wantEmail && hasEmail);

            if (hasOneOrMoreResult)
            {
                wantedData.AppendLine($"Voici les données demandées pour {crmState.Lead.FullName} : ");
                if (wantCompany)
                {
                    if (!hasCompany)
                    {
                        missingWantedData += "compagnie. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"Compagnie : {crmState.Lead.Company}.");
                    }
                }

                if (wantAddress)
                {
                    if (!hasAddress)
                    {
                        missingWantedData += "adresse du domicile. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"Adresse du domicile : {crmState.Lead.Address}.");
                    }
                }

                if (wantPhone)
                {
                    if (!hasPhone)
                    {
                        missingWantedData += "numéro de téléphone. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"Numéro de téléphone : {crmState.Lead.PhoneNumber}.");
                    }
                }

                if (wantEmail)
                {
                    if (!hasEmail)
                    {
                        missingWantedData += "adresse email. ";
                    }
                    else
                    {
                        wantedData.AppendLine($"Adresse email : {crmState.Lead.Email}.");
                    }
                }
            }
            else
            {
                wantedData.Append($"{crmState.Lead.FullName} est bien référencé dans la base de données mais les informations demandées sont absentes.");
            }
            
            if(!string.IsNullOrEmpty(missingWantedData))
            {
                missingWantedData = "Les données suivantes sont absentes de la base de données : " + missingWantedData;
            }
            return $"{wantedData.ToString()} {missingWantedData}";
        }

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await CRMStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            var isSearchData = luisState.IntentName == Intents.SearchData;
            var wantPhone = luisState.Entities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
            var hasPhoneNumber = !string.IsNullOrEmpty(crmState.Lead.PhoneNumber);
            var forward = false;

            if (isSearchData)
            {
                if(wantPhone && hasPhoneNumber && luisState.Entities.Count==1)
                {
                    forward = (bool)stepContext.Result;
                }
                if (!forward)
                {
                    //Ending Dialog
                    var message = Properties.strings.welcome_2;
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
            
            //"Forwarding" the call
            var textMessage = Properties.strings.callForwardingConfirmed;
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

            return await stepContext.EndDialogAsync();
        }
    }
}
