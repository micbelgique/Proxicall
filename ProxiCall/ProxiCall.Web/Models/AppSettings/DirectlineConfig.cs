using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProxiCall.Web.Models.AppSettings
{
    public class DirectlineConfig
    {
        [JsonProperty("DirectlineSecret")]
        public string DirectlineSecret { get; set; }

        [JsonProperty("Host")]
        public Uri Host { get; set; }

        [JsonProperty("ProxiCallCrmHostname")]
        public Uri ProxiCallCrmHostname { get; set; }

        [JsonProperty("AdminPhoneNumber")] 
        public string AdminPhoneNumber { get; set; }

        [JsonProperty("BotName")]
        public string BotName { get; set; }
    }
}
