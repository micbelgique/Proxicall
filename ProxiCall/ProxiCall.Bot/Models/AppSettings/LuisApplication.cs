using Newtonsoft.Json;

namespace ProxiCall.Bot.Models.AppSettings
{
    public class LuisApplication
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("AppId")]
        public string AppId { get; set; }

        [JsonProperty("Hostname")]
        public string Hostname { get; set; }

        [JsonProperty("Culture")]
        public string Culture { get; set; }
    }
}
