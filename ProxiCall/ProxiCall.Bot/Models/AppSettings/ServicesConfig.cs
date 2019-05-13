using Newtonsoft.Json;

namespace ProxiCall.Bot.Models.AppSettings
{
    public class ServicesConfig
    {
        [JsonProperty("ProxiCallCrmHostname")]
        public string ProxiCallCrmHostname { get; set; }
    }
}
