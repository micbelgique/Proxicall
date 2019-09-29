using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProxiCall.Bot.Models.AppSettings
{
    public class LuisConfig
    {
        [JsonProperty("ApiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("Hostname")]
        public string Hostname { get; set; }

        [JsonProperty("LuisApplications")]
        public List<LuisApplicationConfig> LuisApplications { get; set; }
    }
}
