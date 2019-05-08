// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams.Middlewares;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxiCall.Bot.Dialogs.Shared;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
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
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;
            if (!File.Exists(botFilePath))
            {
                throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
            }

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = null;
            try
            {
                botConfig = BotConfiguration.Load(botFilePath, secretKey);
            }
            catch
            {
                var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
                            - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
                            - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
                            - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
                            ";
                throw new InvalidOperationException(msg);
            }

            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. botFilePath: {botFilePath}"));

            // Add BotServices singleton.
            // Create the connected services from .bot file.
            services.AddSingleton(sp => new BotServices(botConfig));

            // Retrieve current endpoint.
            var environment = _isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (service == null && _isProduction)
            {
                // Attempt to load development environment
                service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
            }

            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
            }

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

            services.AddBot<ProxiCallBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Catches any errors that occur during a conversation turn and logs them to currently
                // configured ILogger.
                ILogger logger = _loggerFactory.CreateLogger<ProxiCallBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    var errorMessage = ExceptionHandler(exception);
                    
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync(errorMessage);
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
