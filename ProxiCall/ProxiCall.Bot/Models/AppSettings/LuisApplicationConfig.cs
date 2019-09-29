using Newtonsoft.Json;

namespace ProxiCall.Bot.Models.AppSettings
{
    public class LuisApplicationConfig
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("AppId")]
        public string AppId { get; set; }

        [JsonProperty("Culture")]
        public string Culture { get; set; }
    }
}
