// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams.Middlewares;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models.AppSettings;
using ProxiCall.Bot.Resources;
using ProxiCall.Bot.Services.ProxiCallCRM;

namespace ProxiCall.Bot
{
    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private readonly bool _isProduction;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration that represents a set of key/value application configuration properties.
        /// </summary>
        /// <value>
        /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// <param name="services">Specifies the contract for a <see cref="IServiceCollection"/> of service descriptors.</param>
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Memory Storage is for local bot debugging only. When the bot
            // is restarted, everything stored in memory will be gone.
            IStorage dataStore = new MemoryStorage();

            // For production bots use the Azure Blob or
            // Azure CosmosDB storage providers. For the Azure
            // based storage providers, add the Microsoft.Bot.Builder.Azure
            // Nuget package to your solution. That package is found at:
            // https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/
            // Un-comment the following lines to use Azure Blob Storage
            // // Storage configuration name or ID from the .bot file.
            // const string StorageConfigurationId = "<STORAGE-NAME-OR-ID-FROM-BOT-FILE>";
            // var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
            // if (!(blobConfig is BlobStorageService blobStorageConfig))
            // {
            //    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
            // }
            // // Default container name.
            // const string DefaultBotContainer = "<DEFAULT-CONTAINER>";
            // var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
            // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);

            // Create and add conversation state.
            var conversationState = new ConversationState(dataStore);
            services.AddSingleton(conversationState);

            var userState = new UserState(dataStore);
            services.AddSingleton(userState);

            var privateConversationState = new PrivateConversationState(dataStore);
            services.AddSingleton(privateConversationState);

            services.AddSingleton<StateAccessors>(sp => new StateAccessors(userState, conversationState, privateConversationState)
            {
                DialogStateAccessor = conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                LoggedUserAccessor = privateConversationState.CreateProperty<LoggedUserState>(nameof(LoggedUserState)),
                LuisStateAccessor = userState.CreateProperty<LuisState>(nameof(LuisState)),
                CRMStateAccessor = userState.CreateProperty<CRMState>(nameof(CRMState))
            });

            services.AddOptions();
            
            // Load settings from appsettings.json
            services.Configure<BotConfig>(Configuration.GetSection("BotConfig"));
            services.Configure<LuisConfig>(Configuration.GetSection("LuisConfig"));
            services.Configure<ServicesConfig>(Configuration.GetSection("ServicesConfig"));

            services.AddSingleton<BotServices>(sp => ActivatorUtilities.CreateInstance<BotServices>(sp));

            services.AddBot<ProxiCallBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(Configuration.GetSection("BotConfig")["MicrosoftAppId"], Configuration.GetSection("BotConfig")["MicrosoftAppPassword"]);

                // Catches any errors that occur during a conversation turn and logs them to currently
                // configured ILogger.
                ILogger logger = _loggerFactory.CreateLogger<ProxiCallBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    var errorMessage = ExceptionHandler(exception);
                    logger.LogError($"Exception caught : {exception}");

                    var activity = MessageFactory.Text(errorMessage, errorMessage, InputHints.AcceptingInput);
                    activity.Locale = CulturedBot.Culture?.Name;
                    await context.SendActivityAsync(activity);
                };
                
                //Adding Teams middleware
                options.Middleware.Add(
                    new TeamsMiddleware(
                        new ConfigurationCredentialProvider(this.Configuration)
                    )
                );
            });
            
            services.AddHttpClient<AccountService>();
            services.AddHttpClient<CompanyService>();
            services.AddHttpClient<LeadService>();
            services.AddHttpClient<OpportunityService>();
            services.AddHttpClient<ProductService>();
            
            services.AddLocalization(options => options.ResourcesPath = "Resources");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }

        private string ExceptionHandler(Exception exception)
        {
            var errorMessage = string.Empty;
            switch (exception)
            {
                // TODO add error messages to resx
                case UserNotFoundException _:
                    errorMessage = "Aucun utilisateur n'a été trouvé";
                    break;
                case InvalidTokenException _:
                    errorMessage = "L'authentification a échoué";
                    break;
                case AccessForbiddenException _:
                    errorMessage = "Vous n'avez pas accès à cette ressource";
                    break;
                case OpportunityNotCreatedException _:
                    errorMessage = "Une erreur est survenue lors de la création de cette opportunité";
                    break;
                case OpportunitiesNotFoundException _:
                    errorMessage = "Aucune opportunité correspondante n'a été trouvée";
                    break;
                case ProductNotFoundException _:
                    errorMessage = "Aucun produit n'a été trouvé";
                    break;
                case LeadNotFoundException _:
                    errorMessage = "Aucun lead n'a été trouvé";
                    break;
                case OwnerNotFoundException _:
                    errorMessage = "Aucun owner n'a été trouvé";
                    break;
                case CompanyNotFoundException _:
                    errorMessage = "Aucune compagnie n'a été trouvée";
                    break;
                default:
                    errorMessage = $"Sorry, it looks like something went wrong : {exception.Message}";
                    break;
            }

            return errorMessage;
        }
    }
}
