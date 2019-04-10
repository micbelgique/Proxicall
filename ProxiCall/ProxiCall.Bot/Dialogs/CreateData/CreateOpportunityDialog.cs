using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services;
using ProxiCall.Bot.Services.ProxiCallCRM;

namespace ProxiCall.Bot.Dialogs.CreateData
{
    public class CreateOpportunityDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StateAccessors _accessors;

        private const string _createOpportunityDataWaterfall = "createOpportunityDataWaterfall";
        //Searching for lead
        private const string _leadFullNamePrompt = "leadFullNamePrompt";
        private const string _retryFetchingLeadFromUserPrompt = "retryFetchingLeadFromUserPrompt";
        //Searching for product
        private const string _productNamePrompt = "ProductNamePrompt";
        private const string _retryFetchingProductFromUserPrompt = "retryFetchingProductFromUserPrompt";
        //Checking the closing date
        private const string _closingDatePrompt = "closingDatePrompt";
        private const string _retryFetchingClosingDateFromUserPrompt = "retryFetchingClosingDateFromUserPrompt";


        public CreateOpportunityDialog(StateAccessors accessors, ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(CreateOpportunityDialog))
        {
            _accessors = accessors;
            _loggerFactory = loggerFactory;
            _botServices = botServices;

            var waterfallSteps = new WaterfallStep[]
            {
                //Start of dialog
                InitializeStateStepAsync,
                //Searching for lead
                AskForLeadFullNameStepAsync,
                SearchLeadStepAsync,
                LeadResultHandlerStepAsync,
                //Searching for product
                AskForProductNameStepAsync,
                SearchProductStepAsync,
                ProductResultHandlerStepAsync,
                //Checking the closing date
                AskForClosingDateStepAsync,
                SearchClosingDateStepAsync,
                ClosingDateResultHandlerStepAsync,
                //End of Dialog
                EndSearchDialogStepAsync

            };
            AddDialog(new WaterfallDialog(_createOpportunityDataWaterfall, waterfallSteps));
            //Searching for lead
            AddDialog(new TextPrompt(_leadFullNamePrompt));
            AddDialog(new ConfirmPrompt(_retryFetchingLeadFromUserPrompt, defaultLocale: "fr-fr"));
            //Searching for product
            AddDialog(new TextPrompt(_productNamePrompt));
            AddDialog(new ConfirmPrompt(_retryFetchingProductFromUserPrompt, defaultLocale: "fr-fr"));
            //Checking the closing date
            AddDialog(new TextPrompt(_closingDatePrompt));
            AddDialog(new ConfirmPrompt(_retryFetchingClosingDateFromUserPrompt, defaultLocale: "fr-fr"));
        }

        //-------------------------
        //-----Start of dialog-----
        //-------------------------
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
        
        //-------------------------
        //-----Search for lead-----
        //-------------------------
        private async Task<DialogTurnResult> AskForLeadFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context);

            if(!string.IsNullOrEmpty(crmState.Lead.FullName))
            {
                crmState.Opportunity.Lead = crmState.Lead;
            }

            //Asking for the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Opportunity.Lead.FullName))
            {
                return await stepContext.PromptAsync(_leadFullNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.AskForWhichLeadToCreateOpportunity)
                }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchLeadStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Opportunity.Lead.FullName))
            {
                crmState.Opportunity.Lead.FullName = (string)stepContext.Result;
            }

            //Searching the lead
            var fullNameGivenByUser = crmState.Opportunity.Lead.FullName;
            crmState.Opportunity.Lead = await SearchLeadAsync(stepContext.Context, crmState.Opportunity.Lead.FirstName, crmState.Opportunity.Lead.LastName);

            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Opportunity.Lead == null)
            {
                promptMessage = $"{string.Format(CulturedBot.NamedObjectNotFound, fullNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
            }

            var needsRetry = !string.IsNullOrEmpty(promptMessage);
            if (needsRetry)
            {
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingLeadFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        //Searching Lead in Database
        private async Task<Lead> SearchLeadAsync(ITurnContext turnContext, string firstName, string lastName)
        {
            var user = await _accessors.LoggedUserAccessor.GetAsync(turnContext, () => new LoggedUserState());
            var leadService = new LeadService(user.LoggedUser.Token);
            return await leadService.GetLeadByName(firstName, lastName);
        }

        private async Task<DialogTurnResult> LeadResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Handling when lead not found
            if (crmState.Opportunity.Lead == null)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetLead();
                    crmState.Opportunity.ResetLead();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    return await stepContext.ReplaceDialogAsync(_createOpportunityDataWaterfall, cancellationToken);
                }
                else
                {
                    //Ending Dialog if user decides not to retry
                    var message = CulturedBot.AskForRequest;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );

                    crmState.ResetOpportunity();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            return await stepContext.NextAsync();
        }

        //----------------------------
        //-----Search for product-----
        //----------------------------
        private async Task<DialogTurnResult> AskForProductNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context);

            if(!string.IsNullOrEmpty(crmState.Product.Title))
            {
                crmState.Opportunity.Product = crmState.Product;
            }

            //Asking for the name of the product if not already given
            if (string.IsNullOrEmpty(crmState.Opportunity.Product.Title))
            {
                return await stepContext.PromptAsync(_productNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.AskForWhichProduct)
                }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Gathering the name of the product if not already given
            if (string.IsNullOrEmpty(crmState.Opportunity.Product.Title))
            {
                crmState.Opportunity.Product.Title = (string)stepContext.Result;
            }

            //Searching the product
            var productNameGivenByUser = crmState.Opportunity.Product.Title;
            crmState.Opportunity.Product = await SearchProductAsync(stepContext.Context, crmState.Opportunity.Product.Title);

            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Opportunity.Product == null)
            {
                promptMessage = $"{string.Format(CulturedBot.NamedObjectNotFound, productNameGivenByUser)} {CulturedBot.AskIfWantRetry}";
            }

            var needsRetry = !string.IsNullOrEmpty(promptMessage);
            if (needsRetry)
            {
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingProductFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        //Searching Product in Database
        private async Task<Product> SearchProductAsync(ITurnContext turnContext, string productName)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(turnContext, () => new LoggedUserState());
            var productService = new ProductService(userState.LoggedUser.Token);
            return await productService.GetProductByTitle(productName);
        }

        private async Task<DialogTurnResult> ProductResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Handling when product not found
            if (crmState.Opportunity.Product == null)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetProduct();
                    crmState.Opportunity.ResetProduct();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    return await stepContext.ReplaceDialogAsync(_createOpportunityDataWaterfall, cancellationToken);
                }
                else
                {
                    //Ending Dialog if user decides not to retry
                    var message = CulturedBot.AskForRequest;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );

                    crmState.ResetOpportunity();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }
            return await stepContext.NextAsync();
        }

        //-----------------------------------
        //-----Checking the closing date-----
        //-----------------------------------

        private async Task<DialogTurnResult> AskForClosingDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context);

            //Asking for date if not already given
            if (crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                return await stepContext.PromptAsync(_closingDatePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.AskEstimatedClosingDateOfOpportunity)
                }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchClosingDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            RecognizerResult luisResult;
            //Gathering the date if not already given
            if (crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                luisResult = await _botServices.LuisServices[BotServices.LUIS_APP_NAME].RecognizeAsync(stepContext.Context, cancellationToken);

                var entities = luisResult.Entities;
                string timex = (string)entities["datetime"]?[0]?["timex"]?.First;
                crmState.Opportunity.EstimatedCloseDate = FormatConvertor.TimexToDateTime(timex);
            }
            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                promptMessage = $"{CulturedBot.AdmitNotUnderstanding} {CulturedBot.AskIfWantToSkip}";
            }

            userState.IsEligibleForPotentialSkippingStep = !string.IsNullOrEmpty(promptMessage);
            if (userState.IsEligibleForPotentialSkippingStep)
            {
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingClosingDateFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> ClosingDateResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            //Handling when date not found
            if (userState.IsEligibleForPotentialSkippingStep)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    return await stepContext.ReplaceDialogAsync(_createOpportunityDataWaterfall, cancellationToken);
                }
                else
                {
                    //Ending Dialog if user decides not to retry
                    var message = CulturedBot.AskForRequest;
                    await stepContext.Context.SendActivityAsync(MessageFactory
                        .Text(message, message, InputHints.AcceptingInput)
                        , cancellationToken
                    );

                    userState.IsEligibleForPotentialSkippingStep = false;
                    crmState.ResetOpportunity();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState);
                    return await stepContext.EndDialogAsync();
                }
            }
            return await stepContext.NextAsync();
        }
        
        //-----------------------
        //-----End of dialog-----
        //-----------------------
        private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState());

            //TODO : take off hardcode
            crmState.Opportunity.Status = "Ouvert";
            crmState.Opportunity.OwnerId = userState.LoggedUser.Id;
            var opportunityService = new OpportunityService(userState.LoggedUser.Token);
            await opportunityService.PostOpportunityAsync(crmState.Opportunity);

            var message = $"{ CulturedBot.SayOpportunityWasCreated} {CulturedBot.AskForRequestAgain}";
            await stepContext.Context.SendActivityAsync(MessageFactory
                .Text(message, message, InputHints.AcceptingInput)
                , cancellationToken
            );

            crmState.ResetOpportunity();
            luisState.ResetAll();
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
            return await stepContext.EndDialogAsync();
        }
    }
}
