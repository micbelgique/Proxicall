using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services.ProxiCallCRM;
using ProxiCall.Library.Enumeration.Opportunity;
using ProxiCall.Library.Services;

namespace ProxiCall.Bot.Dialogs.CreateData
{
    public class CreateOpportunityDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StateAccessors _accessors;
        private readonly LeadService _leadService;
        private readonly OpportunityService _opportunityService;
        private readonly ProductService _productService;

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
        //Checking for comment
        private const string _commentPrompt = "commentPrompt";
        private const string _fetchingCommentFromUserPrompt = "fetchingCommentFromUserPrompt";


        public CreateOpportunityDialog(StateAccessors accessors, ILoggerFactory loggerFactory,
            BotServices botServices,IServiceProvider serviceProvider) : base(nameof(CreateOpportunityDialog))
        {
            _accessors = accessors;
            _loggerFactory = loggerFactory;
            _botServices = botServices;
            _serviceProvider = serviceProvider;

            _leadService = (LeadService)_serviceProvider.GetService(typeof(LeadService));
            _opportunityService = (OpportunityService) _serviceProvider.GetService(typeof(OpportunityService));
            _productService = (ProductService) _serviceProvider.GetService(typeof(ProductService));

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
                //Checking for comment
                AskIfUserWantsToCommentStepAsync,
                AskToCommentStepAsync,
                FetchingCommentFromUserStepAsync,
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
            //Checking for comment
            AddDialog(new ConfirmPrompt(_commentPrompt, defaultLocale: "fr-fr"));
            AddDialog(new TextPrompt(_fetchingCommentFromUserPrompt));
        }

        //-------------------------
        //-----Start of dialog-----
        //-------------------------
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
        
        //-------------------------
        //-----Search for lead-----
        //-------------------------
        private async Task<DialogTurnResult> AskForLeadFullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);

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
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SearchLeadStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Opportunity.Lead.FullName))
            {
                crmState.Opportunity.Lead.FullName = (string)stepContext.Result;
            }

            //Searching the lead
            var fullNameGivenByUser = crmState.Opportunity.Lead.FullName;
            crmState.Opportunity.Lead = await SearchLeadAsync(stepContext.Context, crmState.Opportunity.Lead.FirstName, crmState.Opportunity.Lead.LastName);
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);

            //Asking for retry if necessary
            if (crmState.Opportunity.Lead == null)
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text($"{string.Format(CulturedBot.NamedObjectNotFound, fullNameGivenByUser)} {CulturedBot.AskIfWantRetry}"),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingLeadFromUserPrompt, promptOptions, cancellationToken);
            }
           
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        //Searching Lead in Database
        private async Task<Lead> SearchLeadAsync(ITurnContext turnContext, string firstName, string lastName)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(turnContext, () => new LoggedUserState());
            return await _leadService.GetLeadByName(userState.LoggedUser.Token, firstName, lastName);
        }

        private async Task<DialogTurnResult> LeadResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);

            //Handling when lead not found
            if (crmState.Opportunity.Lead == null)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetLead();
                    crmState.Opportunity.ResetLead();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(_createOpportunityDataWaterfall, cancellationToken, cancellationToken);
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
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        //----------------------------
        //-----Search for product-----
        //----------------------------
        private async Task<DialogTurnResult> AskForProductNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);
            
            //Asking for the name of the product if not already given
            if (string.IsNullOrEmpty(crmState.Product.Title))
            {
                return await stepContext.PromptAsync(_productNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.AskForWhichProduct)
                }, cancellationToken);
            }
            else
            {
                crmState.Opportunity.Product = crmState.Product;
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SearchProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);

            //Gathering the name of the product if not already given
            if (string.IsNullOrEmpty(crmState.Product.Title))
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
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingProductFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        //Searching Product in Database
        private async Task<Product> SearchProductAsync(ITurnContext turnContext, string productName)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(turnContext, () => new LoggedUserState());
            return await _productService.GetProductByTitle(userState.LoggedUser.Token, productName);
        }

        private async Task<DialogTurnResult> ProductResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);

            //Handling when product not found
            if (crmState.Opportunity.Product == null)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.ResetProduct();
                    crmState.Opportunity.ResetProduct();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(_createOpportunityDataWaterfall, cancellationToken, cancellationToken);
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
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        //-----------------------------------
        //-----Checking the closing date-----
        //-----------------------------------

        private async Task<DialogTurnResult> AskForClosingDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);

            //Asking for date if not already given
            if (crmState.Opportunity.EstimatedCloseDate == null || crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                return await stepContext.PromptAsync(_closingDatePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.AskEstimatedClosingDateOfOpportunity)
                }, cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SearchClosingDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState(), cancellationToken);

            //Gathering the date if not already given
            if (crmState.Opportunity.EstimatedCloseDate == null || crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                var luisResult = await _botServices.LuisServices[BotServices.LUIS_APP_NAME].RecognizeAsync(stepContext.Context, cancellationToken);

                var entities = luisResult.Entities;
                string timex = (string)entities["datetime"]?[0]?["timex"]?.First;
                var formatConvertor = new FormatConvertor();
                crmState.Opportunity.EstimatedCloseDate = formatConvertor.TurnTimexToDateTime(timex);
            }
            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Opportunity.EstimatedCloseDate == null || crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                promptMessage = $"{CulturedBot.AdmitNotUnderstanding} {CulturedBot.AskIfWantToSkip}";
            }

            userState.IsEligibleForPotentialSkippingStep = !string.IsNullOrEmpty(promptMessage);
            if (userState.IsEligibleForPotentialSkippingStep)
            {
                await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState, cancellationToken);
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
                };
                return await stepContext.PromptAsync(_retryFetchingClosingDateFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState, cancellationToken);
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ClosingDateResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState(), cancellationToken);

            //Handling when date not found
            if (userState.IsEligibleForPotentialSkippingStep)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(_createOpportunityDataWaterfall, cancellationToken, cancellationToken);
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
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState, cancellationToken);
                    await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        //------------------------------
        //-----Checking for Comment-----
        //------------------------------
        private async Task<DialogTurnResult> AskIfUserWantsToCommentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState(), cancellationToken);
            
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Avez-vous un commentaire a ajouté?"),
                RetryPrompt = MessageFactory.Text(CulturedBot.AskYesOrNo),
            };
            return await stepContext.PromptAsync(_commentPrompt, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AskToCommentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState(), cancellationToken);

            userState.WantsToComment = (bool)stepContext.Result;
            await _accessors.LoggedUserAccessor.SetAsync(stepContext.Context, userState, cancellationToken);

            if (userState.WantsToComment)
            {
                return await stepContext.PromptAsync(_fetchingCommentFromUserPrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text(CulturedBot.SayGoAhead)
                }, cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FetchingCommentFromUserStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState(), cancellationToken);
            if (userState.WantsToComment)
            {
                crmState.Opportunity.Comments = (string)stepContext.Result;
                await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

            //-----------------------
            //-----End of dialog-----
            //-----------------------
            private async Task<DialogTurnResult> EndSearchDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState(), cancellationToken);
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState(), cancellationToken);
            var userState = await _accessors.LoggedUserAccessor.GetAsync(stepContext.Context, () => new LoggedUserState(), cancellationToken);

            //Finalizing the created opportunity
            crmState.Opportunity.Status = OpportunityStatus.Open.Name;
            crmState.Opportunity.OwnerId = userState.LoggedUser.Id;

            var message = string.Empty;
            
            //Posting the created opportunity
            try
            {
                await _opportunityService.PostOpportunityAsync(userState.LoggedUser.Token, crmState.Opportunity);
                message = $"{ CulturedBot.SayOpportunityWasCreated} {CulturedBot.AskForRequestAgain}";
            }
            catch (OpportunityNotCreatedException ex)
            {
                //TODO add message to resx
                message = "La création de l'opportunité a échoué";
            }

            await stepContext.Context.SendActivityAsync(MessageFactory
                .Text(message, message, InputHints.AcceptingInput)
                , cancellationToken
            );

            crmState.ResetOpportunity();
            luisState.ResetAll();
            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState, cancellationToken);
            await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
