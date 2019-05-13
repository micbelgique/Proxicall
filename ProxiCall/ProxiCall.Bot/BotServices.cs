// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License

using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Options;
using ProxiCall.Bot.Models.AppSettings;
using LuisApplication = Microsoft.Bot.Builder.AI.Luis.LuisApplication;

namespace ProxiCall.Bot
{
    /// <summary>
    /// Represents references to external services.
    ///
    /// For example, LUIS services are kept here as a singleton.  This external service is configured
    /// using the <see cref="BotConfiguration"/> class.
    /// </summary>
    //  See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1
    //  for more information regarding dependency injection
    //  See https://www.luis.ai/home" for more information regarding language understanding using LUIS
    public class BotServices
    {
        public LuisConfig LuisConfig { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// <param name="botConfiguration">A dictionary of named <see cref="BotConfiguration"/> instances for usage within the bot.</param>
        /// </summary>
        public BotServices(IOptions<LuisConfig> config)
        {
            LuisConfig = config.Value;

            foreach (var luisApplication in LuisConfig.LuisApplications)
            {
                var app = new LuisApplication(luisApplication.AppId, LuisConfig.ApiKey, LuisConfig.Hostname);
                var recognizer = new LuisRecognizer(app);
                LuisServices.Add(luisApplication.Name, recognizer);
            }
        }

        /// <summary>
        /// Gets the set of LUIS Services used.
        /// Given there can be multiple <see cref="LuisRecognizer"/> services used in a single bot,
        /// LuisServices is represented as a dictionary.  This is also modeled in the
        /// ".bot" file since the elements are named.
        /// <remarks>The LUIS services collection should not be modified while the bot is running.</remarks>
        /// <value>
        /// A <see cref="LuisRecognizer"/> client instance created based on configuration in the .bot file.
        /// </value>
        /// </summary>
        public Dictionary<string, LuisRecognizer> LuisServices { get; } = new Dictionary<string, LuisRecognizer>();
    }
}