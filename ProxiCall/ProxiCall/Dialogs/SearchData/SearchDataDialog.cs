using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProxiCall.Dialogs.Shared;
using ProxiCall.Models;
using ProxiCall.Models.Intents;
using ProxiCall.Services.ProxiCallCRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.SearchData
{
    public class SearchLeadDataDialog : ComponentDialog
    {
        public IStatePropertyAccessor<LeadState> LeadStateAccessor { get; }
        public IStatePropertyAccessor<LuisState> LuisStateAccessor { get; }
        public ILoggerFactory LoggerFactory { get; }
        public BotServices BotServices { get; }

        private const string _searchLeadDataWaterfall = "searchLeadDataWaterfall";
        private const string _leadFullNamePrompt = "leadFullNamePrompt";
        private const string _retryNumberSearchPrompt = "retryNumberSearchPrompt";
        private const string _confirmForwardingPrompt = "confirmForwardingPrompt";

        public SearchLeadDataDialog(IStatePropertyAccessor<LeadState> leadStateAccessor, IStatePropertyAccessor<LuisState> luisStateAccessor,
            ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(SearchLeadDataDialog))
        {
            LeadStateAccessor = leadStateAccessor;
            LuisStateAccessor = luisStateAccessor;
            LoggerFactory = loggerFactory;
            BotServices = botServices;

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskForLeadFullNameStepAsync,
                SearchLeadNumberStepAsync,
                ResultHandlerStepAsync,
                EndSearchDialogStepAsync
            };
            AddDialog(new WaterfallDialog(_searchLeadDataWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(_leadFullNamePrompt));
            AddDialog(new ConfirmPrompt(_retryNumberSearchPrompt, defaultLocale: "fr-fr"));
            AddDialog(new ConfirmPrompt(_confirmForwardingPrompt, defaultLocale: "fr-fr"));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initializing LeadStateAccessor
            var leadState = await LeadStateAccessor.GetAsync(stepContext.Context, () => null);
            if (leadState == null)
            {
                if (stepContext.Options is LeadState callStateOpt)
                {
                    await LeadStateAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await LeadStateAccessor.SetAsync(stepContext.Context, new LeadState());
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
            var leadState = await LeadStateAccessor.GetAsync(stepContext.Context);

            //Asking for the name of the lead if not already given
            if (string.IsNullOrEmpty(leadState.LeadFullName))
            {
                return await stepContext.PromptAsync(_leadFullNamePrompt, new PromptOptions { Prompt = MessageFactory.Text(Properties.strings.querySearchPerson) }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchLeadNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var leadState = await LeadStateAccessor.GetAsync(stepContext.Context);

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(leadState.LeadFullName))
            {
                leadState.LeadFullName = (string)stepContext.Result;
            }

            //Searching the lead
            leadState.Lead = await SearchLeadAsync(leadState.LeadFirstName, leadState.LeadLastName);

            //Asking for retry if lead not found
            //TODO : en fonction de si number trouvé ou pas
            if (leadState.Lead == null)
            {
                leadState.PhoneNumber = "";
                var promptMessage = $"{leadState.LeadFullName} {Properties.strings.retryNumberSearchPrompt}";
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
                };
                return await stepContext.PromptAsync(_retryNumberSearchPrompt, promptOptions, cancellationToken);
            }

            //Moving on to the next step if lead found
            leadState.PhoneNumber = leadState.Lead.PhoneNumber;
            leadState.Address = leadState.Lead.Address;
            leadState.Company = leadState.Lead.Company;
            await LeadStateAccessor.SetAsync(stepContext.Context, leadState);

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
            var leadState = await LeadStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            //Handling when lead not found
            if (leadState.Lead == null)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    leadState.Reset();
                    await LeadStateAccessor.SetAsync(stepContext.Context, leadState);
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

                    leadState.Reset();
                    luisState.ResetAll();
                    await LeadStateAccessor.SetAsync(stepContext.Context, leadState);
                    await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Giving informations to User
            if (luisState.IntentName == Intents.SearchData)
            {
                //Creating adapted response
                var wantPhone = luisState.DetectedEntities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
                var wantAddress = luisState.DetectedEntities.Contains(LuisState.SEARCH_ADDRESS_ENTITYNAME);
                var wantCompany = luisState.DetectedEntities.Contains(LuisState.SEARCH_COMPANY_ENTITYNAME);

                var textMessage = "";
                var phoneFragment = "";
                var addressFragment = "";
                var companyFragment = "";
                
                if (wantCompany)
                {
                    if (string.IsNullOrEmpty(leadState.Company))
                    {
                        companyFragment = "ne semble pas avoir de compagnie répertoriée";
                    }
                    else if (luisState.DetectedEntities.Count == 1)
                    {
                        companyFragment = $"travaille pour {leadState.Company}";
                    }
                    else
                    {
                        companyFragment = $"de {leadState.Company}";
                    }
                }

                if (wantPhone)
                {
                    if (string.IsNullOrEmpty(leadState.PhoneNumber))
                    {
                        phoneFragment = "ne semble pas avoir de numéro répertorié";
                    }
                    else if (wantCompany)
                    {
                        var hasCompany = !string.IsNullOrEmpty(leadState.Company);
                        if (hasCompany)
                        {
                            phoneFragment = $"a pour numéro de téléphone le {leadState.PhoneNumber}";
                        }
                        else
                        {
                            phoneFragment = $", son numéro de téléphone est le {leadState.PhoneNumber}";
                        }
                    }
                    else 
                    {
                        if (wantAddress)
                        {
                            phoneFragment = "et ";
                        }
                        phoneFragment += $"a pour numéro de téléphone le {leadState.PhoneNumber}";
                    }
                }

                if (wantAddress)
                {
                    if (string.IsNullOrEmpty(leadState.Address))
                    {
                        addressFragment = "ne semble pas avoir d'adresse répertoriée";
                    }
                    else
                    {
                        if(luisState.DetectedEntities.Count>1)
                        {
                            addressFragment = ", ";
                        }
                        addressFragment += $"habite {leadState.Address}";
                    }
                }

                textMessage = $"{leadState.LeadFullName} {companyFragment} {addressFragment} {phoneFragment}";

                //Sending response
                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                        .Text(textMessage, textMessage, InputHints.IgnoringInput)
                        , cancellationToken
                );

                //Asking if user wants to forward the call
                if(wantPhone && !string.IsNullOrEmpty(leadState.PhoneNumber))
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

        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var leadState = await LeadStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            var isSearchData = luisState.IntentName == Intents.SearchData;
            var wantPhone = luisState.DetectedEntities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
            var hasPhoneNumber = !string.IsNullOrEmpty(leadState.PhoneNumber);
            var forward = false;

            if (isSearchData)
            {
                if(wantPhone && hasPhoneNumber)
                {
                    forward = (bool)stepContext.Result;
                }
                if (!forward)
                {
                    //Ending PhoneNumberDialog
                    var message = Properties.strings.welcome_2;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );
                    leadState.Reset();
                    luisState.ResetAll();
                    await LeadStateAccessor.SetAsync(stepContext.Context, leadState);
                    await LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }
            
            //"Forwarding" the call
            var textMessage = Properties.strings.callForwardingConfirmed;
            Activity activity = MessageFactory.Text(textMessage, textMessage, InputHints.IgnoringInput);
            var entity = new Entity();
            entity.Properties.Add("forward", JToken.Parse(leadState.PhoneNumber));
            activity.Entities.Add(entity);

            await stepContext.Context.SendActivityAsync(activity, cancellationToken);

            leadState.Reset();
            luisState.ResetAll();
            luisState.ResetIntentIfNoEntities();
            await LeadStateAccessor.SetAsync(stepContext.Context, leadState);
            await LuisStateAccessor.SetAsync(stepContext.Context, luisState);

            return await stepContext.EndDialogAsync();
        }
    }
}
