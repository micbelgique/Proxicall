using Newtonsoft.Json;

namespace ProxiCall.Bot.Models.AppSettings
{
    public class BotConfig
    {
        [JsonProperty("MicrosoftAppId")]
        public string MicrosoftAppId { get; set; }

        [JsonProperty("MicrosoftAppPassword")]
        public string MicrosoftAppPassword { get; set; }
    }
}
