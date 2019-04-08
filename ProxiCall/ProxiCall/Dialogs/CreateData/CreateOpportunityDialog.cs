using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using ProxiCall.Dialogs.Shared;
using ProxiCall.Models;
using ProxiCall.Models.Intents;
using ProxiCall.Resources;
using ProxiCall.Services;
using ProxiCall.Services.ProxiCallCRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.CreateData
{
    public class CreateOpportunityDialog : ComponentDialog
    {
        private readonly BotServices _botServices;
        private readonly ILoggerFactory _loggerFactory;
        private readonly StateAccessors _accessors;

        private const string _searchLeadDataWaterfall = "searchLeadDataWaterfall";
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
                ClosingDateResultHandlerStepAsync

            };
            AddDialog(new WaterfallDialog(_searchLeadDataWaterfall, waterfallSteps));
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
            var currentUser = await _accessors.UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (currentUser == null)
            {
                if (stepContext.Options is UserProfile callStateOpt)
                {
                    await _accessors.UserProfileAccessor.SetAsync(stepContext.Context, callStateOpt);
                }
                else
                {
                    await _accessors.UserProfileAccessor.SetAsync(stepContext.Context, new UserProfile());
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

            //Asking for the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Lead.FullName))
            {
                return await stepContext.PromptAsync(_leadFullNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Pour quel client désirez-vous créer cette opportunité?")
                }, cancellationToken);
            }
            else
            {
                crmState.Opportunity.Lead = crmState.Lead;
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchLeadStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Gathering the name of the lead if not already given
            if (string.IsNullOrEmpty(crmState.Lead.FullName))
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
            var user = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            var leadService = new LeadService(user.Token);
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
                    crmState.Opportunity.ResetLead();
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

                    crmState.Opportunity.ResetLead();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Introducing next step
            var textMessage = $"Une opportunité sera créée pour {crmState.Opportunity.Lead.FullName}.";

            //Sending response
            await stepContext.Context
                .SendActivityAsync(MessageFactory
                    .Text(textMessage, textMessage, InputHints.IgnoringInput)
                    , cancellationToken
            );

            return await stepContext.NextAsync();
        }

        //----------------------------
        //-----Search for product-----
        //----------------------------
        private async Task<DialogTurnResult> AskForProductNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context);

            //Asking for the name of the product if not already given
            if (string.IsNullOrEmpty(crmState.Opportunity.Product.Title))
            {
                return await stepContext.PromptAsync(_productNamePrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Pour quel produit désirez-vous créer une opportunité?")
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
            var productNameGivenByUser = crmState.Lead.FullName;
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
            var user = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            var productService = new ProductService(user.Token);
            return await productService.GetProductByTitle(productName);
        }

        private async Task<DialogTurnResult> ProductResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Handling when lead not found
            if (crmState.Opportunity.Product == null)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
                    crmState.Opportunity.ResetProduct();
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

                    crmState.Opportunity.ResetLead();
                    crmState.Opportunity.ResetProduct();
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Introducing next step
            var textMessage = $"Une opportunité sera créée pour {crmState.Opportunity.Product.Title}.";

            //Sending response
            await stepContext.Context
                .SendActivityAsync(MessageFactory
                    .Text(textMessage, textMessage, InputHints.IgnoringInput)
                    , cancellationToken
            );

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
                    Prompt = MessageFactory.Text("A quelle date pourrait être conclue cette opportunité?")
                }, cancellationToken);
            }
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SearchClosingDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            RecognizerResult luisResult;
            //Gathering the date if not already given
            if (crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                luisResult = await _botServices.LuisServices["proxicall-luis"].RecognizeAsync(stepContext.Context, cancellationToken);

                var entities = luisResult.Entities;
                string timex = (string)entities["datetime"]?[0]?["timex"]?.First;
                crmState.Opportunity.EstimatedCloseDate = FormatConvertor.TimexToDateTime(timex);
            }
            //Asking for retry if necessary
            var promptMessage = "";
            if (crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                promptMessage = $"Aucune Date reconnue {CulturedBot.AskIfWantRetry}";
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
                return await stepContext.PromptAsync(_retryFetchingClosingDateFromUserPrompt, promptOptions, cancellationToken);
            }

            await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> ClosingDateResultHandlerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var crmState = await _accessors.CRMStateAccessor.GetAsync(stepContext.Context, () => new CRMState());
            var luisState = await _accessors.LuisStateAccessor.GetAsync(stepContext.Context, () => new LuisState());

            //Handling when lead not found
            if (crmState.Opportunity.EstimatedCloseDate == DateTime.MinValue)
            {
                var retry = (bool)stepContext.Result;
                if (retry)
                {
                    //Restarting dialog if user decides to retry
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

                    crmState.Opportunity.ResetLead();
                    crmState.Opportunity.ResetProduct();
                    crmState.Opportunity.EstimatedCloseDate = DateTime.MinValue;
                    luisState.ResetAll();
                    await _accessors.CRMStateAccessor.SetAsync(stepContext.Context, crmState);
                    await _accessors.LuisStateAccessor.SetAsync(stepContext.Context, luisState);
                    return await stepContext.EndDialogAsync();
                }
            }

            //Introducing next step
            var textMessage = $"Une opportunité sera créée le {crmState.Opportunity.EstimatedCloseDate}.";

            //Sending response
            await stepContext.Context
                .SendActivityAsync(MessageFactory
                    .Text(textMessage, textMessage, InputHints.IgnoringInput)
                    , cancellationToken
            );

            return await stepContext.NextAsync();
        }


        //-----------------------
        //-----End of dialog-----
        //-----------------------
    }
}
