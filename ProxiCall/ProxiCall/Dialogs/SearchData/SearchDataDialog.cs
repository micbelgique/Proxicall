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
    public class SearchDataDialog : ComponentDialog
    {
        public IStatePropertyAccessor<LeadState> LeadStateAccessor { get; }
        public IStatePropertyAccessor<LuisState> LuisStateAccessor { get; }
        public ILoggerFactory LoggerFactory { get; }
        public BotServices BotServices { get; }

        private const string PhoneNumberWaterfall = "phoneNumberWaterfall";
        private const string LeadFullNamePrompt = "leadFullNamePrompt";
        private const string RetryNumberSearchPrompt = "retryNumberSearchPrompt";
        private const string ConfirmForwardingPrompt = "confirmForwardingPrompt";

        public SearchDataDialog(IStatePropertyAccessor<LeadState> leadStateAccessor, IStatePropertyAccessor<LuisState> luisStateAccessor,
            ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(SearchDataDialog))
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
                EndTelExchangeDialogStepAsync
            };
            AddDialog(new WaterfallDialog(PhoneNumberWaterfall, waterfallSteps));
            AddDialog(new TextPrompt(LeadFullNamePrompt));
            AddDialog(new ConfirmPrompt(RetryNumberSearchPrompt, defaultLocale: "fr-fr"));
            AddDialog(new ConfirmPrompt(ConfirmForwardingPrompt, defaultLocale: "fr-fr"));
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
                return await stepContext.PromptAsync(LeadFullNamePrompt, new PromptOptions { Prompt = MessageFactory.Text(Properties.strings.querySearchPerson) }, cancellationToken);
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

            //Searching the phone number
            var lead = await SearchLeadAsync(leadState.LeadFirstName, leadState.LeadLastName);

            //Asking for retry if lead not found
            //TODO : en fonction de si number trouvé ou pas
            if (lead == null)
            {
                var promptMessage = $"{leadState.LeadFullName} {Properties.strings.retryNumberSearchPrompt}";
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
                };
                return await stepContext.PromptAsync(RetryNumberSearchPrompt, promptOptions, cancellationToken);
            }

            //Moving on to the next step if lead found
            leadState.PhoneNumber = lead.PhoneNumber;
            leadState.Address = lead.Address;
            leadState.Company = lead.Company;
            await LeadStateAccessor.SetAsync(stepContext.Context, leadState);

            return await stepContext.NextAsync();
        }

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
            if (string.IsNullOrEmpty(leadState.PhoneNumber))
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    leadState.Reset();
                    await LeadStateAccessor.SetAsync(stepContext.Context, leadState);
                    return await stepContext.ReplaceDialogAsync(PhoneNumberWaterfall, cancellationToken);
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
                
                if (wantCompany && !wantPhone)
                {
                    if (string.IsNullOrEmpty(leadState.Company))
                    {
                        companyFragment = "ne semble pas avoir de compagnie répertoriée";
                    }
                    else
                    {
                        companyFragment = $"de {leadState.Company}";
                    }
                }

                if (wantPhone)
                {
                    if (string.IsNullOrEmpty(leadState.Company))
                    {
                        companyFragment = "ne semble pas avoir de numéro répertoriée";
                    }
                    else
                    {
                        if (wantPhone && wantCompany)
                        {
                            if (string.IsNullOrEmpty(leadState.Company))
                            {
                                companyFragment = "ne semble pas avoir de compagnie répertoriée et";
                            }
                            else
                            {
                                companyFragment = $"de {leadState.Company}";
                            }
                            phoneFragment = $"a pour numéro de téléphone le {leadState.PhoneNumber}";
                        }
                        else
                        {
                            phoneFragment = $"et son numéro de téléphone est le {leadState.PhoneNumber}";
                        }
                    }
                }

                if (wantAddress)
                {
                    if (string.IsNullOrEmpty(leadState.Company))
                    {
                        companyFragment = "ne semble pas avoir d'adresse répartorié";
                    }
                    else
                    {
                        companyFragment = $"habite {leadState.Address}";
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
                if(wantPhone)
                {
                    var forwardPromptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text(Properties.strings.forwardCallPrompt),
                        RetryPrompt = MessageFactory.Text(Properties.strings.retryPrompt),
                    };
                    return await stepContext.PromptAsync(ConfirmForwardingPrompt, forwardPromptOptions, cancellationToken);
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> EndTelExchangeDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var leadState = await LeadStateAccessor.GetAsync(stepContext.Context);
            var luisState = await LuisStateAccessor.GetAsync(stepContext.Context);

            if (luisState.IntentName == Intents.SearchData && luisState.DetectedEntities.Contains(LuisState.SEARCH_PHONENUMBER_ENTITYNAME))
            {
                var forward = (bool)stepContext.Result;
                if (!forward)
                {
                    //Ending PhoneNumberDialog
                    var message = Properties.strings.welcome_2;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );
                    leadState.Reset();
                    luisState.RemoveDetectedEntity(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
                    luisState.ResetIntentIfNoEntities();
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
            luisState.RemoveDetectedEntity(LuisState.SEARCH_PHONENUMBER_ENTITYNAME);
            luisState.ResetIntentIfNoEntities();
            await LeadStateAccessor.SetAsync(stepContext.Context, leadState);
            await LuisStateAccessor.SetAsync(stepContext.Context, luisState);

            return await stepContext.EndDialogAsync();
        }
    }
}
