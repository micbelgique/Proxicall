// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProxiCall.Bot.Dialogs.CreateData;
using ProxiCall.Bot.Dialogs.SearchData;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services.ProxiCallCRM;
using ProxiCall.Library.Enumeration.Opportunity;
using ProxiCall.Library.ProxiCallLuis;
using ProxiCall.Library.Services;

namespace ProxiCall.Bot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class ProxiCallBot : IBot
    {
        private readonly BotServices _services;
        private readonly StateAccessors _accessors;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly AccountService _accountService;

        public DialogSet Dialogs { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public ProxiCallBot(BotServices services, StateAccessors accessors, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _accountService = (AccountService) _serviceProvider.GetService(typeof(AccountService));
            
            Dialogs = new DialogSet(_accessors.DialogStateAccessor);
            Dialogs.Add(ActivatorUtilities.CreateInstance<SearchLeadDataDialog>(_serviceProvider));
            Dialogs.Add(ActivatorUtilities.CreateInstance<SearchCompanyDataDialog>(_serviceProvider));
            Dialogs.Add(ActivatorUtilities.CreateInstance<CreateOpportunityDialog>(_serviceProvider));
        }
        
        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var activity = turnContext.Activity;

            // Create a dialog context
            var dialogContext = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
            
            // Check if development environment
            var isDevelopmentEnvironment = activity.ChannelId == "webchat" || activity.ChannelId == "emulator";

            var userState = await _accessors.LoggedUserAccessor.GetAsync(dialogContext.Context, () => new LoggedUserState(), cancellationToken);

            var loginMethod = "phone";
            var isFirstMessage = false;
            var credential = string.Empty;

            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.ChannelId == "directline")
                {
                    if (activity.Entities != null)
                    {
                        foreach (var entity in activity.Entities)
                        {
                            isFirstMessage = entity.Properties.TryGetValue("firstmessage", out var jtoken);
                            credential = isFirstMessage ? jtoken.ToString() : string.Empty;
                            if(isFirstMessage)
                                break;
                        }
                    }
                }
                else if (activity.ChannelId == "msteams")
                {
                    if (activity.Text == "restart")
                    {
                        userState = new LoggedUserState();
                        isFirstMessage = true;
                    }
                    if (userState.LoggedUser.Token == null)
                    {
                        loginMethod = "aad";
                        credential = activity.From.AadObjectId;
                        isFirstMessage = true;
                    }
                }
                else if (userState.LoggedUser.Token == null && isDevelopmentEnvironment)
                {
                    //TODO : when do we land here?
                    // Admin login for development purposes
                    isFirstMessage = true;
                    credential = Environment.GetEnvironmentVariable("AdminPhoneNumber");
                }

                if(isFirstMessage)
                {
                    // This is the first message sent by the bot on production
                    try
                    {
                        var loggedUser = await _accountService.Authenticate(credential, loginMethod);
                        userState.LoggedUser = loggedUser;
                        await _accessors.LoggedUserAccessor.SetAsync(dialogContext.Context, userState,
                            cancellationToken);

                        SwitchCulture(loggedUser.Language);

                        if (!isDevelopmentEnvironment && !string.IsNullOrEmpty(userState.LoggedUser.Token))
                        {
                            var welcomingMessage = $"{string.Format(CulturedBot.Greet,loggedUser.Alias)}. {CulturedBot.AskForRequest}";
                            var replyActivity = MessageFactory.Text(welcomingMessage, welcomingMessage, InputHints.AcceptingInput);
                            replyActivity.Locale = CulturedBot.Culture?.Name;
                            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"{CulturedBot.NoAccessException}";
                        var reply = MessageFactory.Text(errorMessage, errorMessage, InputHints.AcceptingInput);
                        var entity = new Entity();
                        entity.Properties.Add("error", JToken.Parse("{\"hangup\":\"" + ex.Message + "\"}"));
                        reply.Entities.Add(entity);
                        await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                }
                
                if(!isFirstMessage || isDevelopmentEnvironment)
                {
                    // Perform a call to LUIS to retrieve results for the current activity message.
                    var luisResults = await _services.LuisServices[CulturedBot.LuisAppName].RecognizeAsync(dialogContext.Context, cancellationToken);

                    // If any entities were updated, treat as interruption.
                    // For example, "no my name is tony" will manifest as an update of the name to be "tony".
                    var topScoringIntent = luisResults?.GetTopScoringIntent();

                    var topIntent = topScoringIntent.Value.intent;

                    // Continue the current dialog
                    var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                    // If no one has responded,
                    if (!dialogContext.Context.Responded)
                    {
                        // Examine results from active dialog
                        switch (dialogResult.Status)
                        {
                            case DialogTurnStatus.Empty:
                                switch (topIntent)
                                {
                                    case ProxiCallIntents.CreateOpportunity:
                                        await UpdateDialogStatesAsync(luisResults, topIntent, dialogContext.Context);
                                        await dialogContext.BeginDialogAsync(nameof(CreateOpportunityDialog), cancellationToken: cancellationToken);
                                        break;
                                    case ProxiCallIntents.SearchCompanyData:
                                        await UpdateDialogStatesAsync(luisResults, topIntent, dialogContext.Context);
                                        await dialogContext.BeginDialogAsync(nameof(SearchCompanyDataDialog), cancellationToken: cancellationToken);
                                        break;
                                    case ProxiCallIntents.SearchLeadData:
                                    case ProxiCallIntents.MakeACall:
                                        await UpdateDialogStatesAsync(luisResults, topIntent, dialogContext.Context);
                                        await dialogContext.BeginDialogAsync(nameof(SearchLeadDataDialog), cancellationToken: cancellationToken);
                                        break;

                                    case ProxiCallIntents.None:
                                    default:
                                        await dialogContext.Context.SendActivityAsync(CulturedBot.NoIntentFound, cancellationToken: cancellationToken);
                                        break;
                                }

                                break;

                            case DialogTurnStatus.Waiting:
                                // The active dialog is waiting for a response from the user, so do nothing.
                                break;

                            case DialogTurnStatus.Complete:
                                await dialogContext.EndDialogAsync(cancellationToken: cancellationToken);
                                break;

                            default:
                                await dialogContext.CancelAllDialogsAsync(cancellationToken);
                                break;
                        }
                    }
                }
            }
            else if (isDevelopmentEnvironment && activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.FirstOrDefault()?.Id == activity.Recipient.Id)
            {
                credential = Environment.GetEnvironmentVariable("AdminPhoneNumber");
                var loggedUser = await _accountService.Authenticate(credential, loginMethod);
                userState.LoggedUser = loggedUser;
                await _accessors.LoggedUserAccessor.SetAsync(turnContext, userState,
                    cancellationToken);

                SwitchCulture(loggedUser.Language);

                var message = $"{string.Format(CulturedBot.Greet, loggedUser.Alias)} {CulturedBot.AskForRequest}";
                var replyActivity = MessageFactory.Text(message, message, InputHints.AcceptingInput);
                replyActivity.Locale = CulturedBot.Culture?.Name;
                await turnContext.SendActivityAsync(replyActivity, cancellationToken);

                isFirstMessage = false;
            }

            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.PrivateConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        
        private async Task UpdateDialogStatesAsync(RecognizerResult luisResult, string intentName, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                // Get latest States
                var crmState = await _accessors.CRMStateAccessor.GetAsync(turnContext, () => new CRMState());

                var luisState = await _accessors.LuisStateAccessor.GetAsync(turnContext, () => new LuisState());

                var entities = luisResult.Entities;

                // Supported LUIS Entities
                string[] luisExpectedLeadName =
                {
                    "leadfullname",
                    "personNameP"
                };
                string luisExpectedCompanyName = "companyName";
                string luisExpectedDateTime = "datetime";
                string luisExpectedProductTitle = "productTitle";
                string luisExpectedConfidenceOpportunity = "confidenceOpportunity";

                string luisHintSearchLeadAddress = "searchaddress";
                string luisHintSearchLeadCompany = "searchcompany";
                string luisHintSearchLeadPhone = "searchphone";
                string luisHintSearchLeadEmail = "searchemail";

                string luisHintSearchCompanyContact = "searchcontact";
                string luisHintSearchContactName = "searchcontactname";

                string luisHintSearchNumberOpportunites = "searchnumberopportunities";
                string luisHintSearchOpportunites = "searchopportunities";

                //Given Data
                foreach (var name in luisExpectedLeadName)
                {
                    if (entities[name] != null)
                    {
                        var fullName = (string)entities[name][0];
                        if(crmState.Lead == null)
                        {
                            crmState.Lead = new Lead();
                        }
                        crmState.Lead.FullName = fullName;
                        break;
                    }
                }

                if (entities[luisExpectedCompanyName] != null)
                {
                    var companyName = (string)entities[luisExpectedCompanyName][0].First;
                    crmState.Company.Name = companyName;
                }

                if (entities[luisExpectedDateTime] != null)
                {
                    string timex = (string)entities[luisExpectedDateTime]?[0]?["timex"]?.First;
                    var formatConvertor = new FormatConvertor();
                    crmState.Opportunity.EstimatedCloseDate = formatConvertor.TurnTimexToDateTime(timex);
                }

                if (entities[luisExpectedProductTitle] != null)
                {
                    var productTitle = (string)entities[luisExpectedProductTitle][0];
                    crmState.Product.Title = productTitle;
                }

                if(intentName == ProxiCallIntents.CreateOpportunity)
                {
                    if (entities[luisExpectedConfidenceOpportunity] != null)
                    {
                        var confidenceOpportunity = (string)entities[luisExpectedConfidenceOpportunity][0].First;
                        crmState.Opportunity.ChangeConfidenceBasedOnName(confidenceOpportunity);
                    }
                    else
                    {
                        crmState.Opportunity.ChangeConfidenceBasedOnName(OpportunityConfidence.Average.Name);
                    }
                }

                //Hints
                if (entities[luisHintSearchLeadAddress] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_ADDRESS_ENTITYNAME);
                }

                if (entities[luisHintSearchLeadCompany] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_COMPANY_ENTITYNAME);
                }

                if (entities[luisHintSearchLeadPhone] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_PHONENUMBER_ENTITYNAME);
                }

                if (entities[luisHintSearchLeadEmail] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_EMAIL_ENTITYNAME);
                }
                
                if (entities[luisHintSearchCompanyContact] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_CONTACT_ENTITYNAME);
                }

                if (entities[luisHintSearchContactName] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_CONTACT_NAME_ENTITYNAME);
                }
                
                if (entities[luisHintSearchOpportunites] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_OPPORTUNITIES_NAME_ENTITYNAME);
                }
                
                if (entities[luisHintSearchNumberOpportunites] != null)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);
                }

                //Searching for "informations" about leads
                var searchInformationsOnLead =
                    intentName == ProxiCallIntents.SearchLeadData 
                    && (luisState.Entities == null || luisState.Entities.Count == 0);

                var searchInformationsOnCompany =
                   intentName == ProxiCallIntents.SearchCompanyData
                   && (luisState.Entities == null || luisState.Entities.Count == 0);

                var searchInformationsOnContactLead =
                    intentName == ProxiCallIntents.SearchCompanyData
                    && luisState.Entities != null
                    && luisState.Entities.Contains(ProxiCallEntities.SEARCH_CONTACT_ENTITYNAME)
                    && luisState.Entities.Count == 1;

                if (searchInformationsOnLead)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_COMPANY_ENTITYNAME);
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);
                }

                if(searchInformationsOnCompany)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);
                }

                if(searchInformationsOnContactLead)
                {
                    luisState.AddDetectedEntity(ProxiCallEntities.SEARCH_NUMBER_OPPORTUNITIES_ENTITYNAME);
                }

                // Set the new values into state.
                luisState.IntentName = intentName;
                await _accessors.CRMStateAccessor.SetAsync(turnContext, crmState);
                await _accessors.LuisStateAccessor.SetAsync(turnContext, luisState);
            }
        }

        private void SwitchCulture(string cultureName)
        {
            var acceptedCultureNames = new string[]
            {
                "en",
                "fr",
                "fr-fr",
                "fr-ca",
                "en-us",
                "en-uk"
            };

            if(!acceptedCultureNames.Contains(cultureName.ToLower()))
            {
                cultureName = "en";
            }
            CulturedBot.Culture = new CultureInfo(cultureName);
            OpportunityConfidenceValue.Culture = new CultureInfo(cultureName);
            OpportunityStatusValue.Culture = new CultureInfo(cultureName);

            if (!_services.LuisServices.ContainsKey(CulturedBot.LuisAppName))
            {
                throw new System.ArgumentException($"The bot configuration does not contain a service type of `luis` with the id `{CulturedBot.LuisAppName}`.");
            }
        }
    }
}