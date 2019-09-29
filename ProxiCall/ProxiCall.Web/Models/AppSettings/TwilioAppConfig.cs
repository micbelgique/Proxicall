using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProxiCall.Web.Models.AppSettings
{
    public class TwilioAppConfig
    {
        [JsonProperty("TwilioSid")]
        public string TwilioSid { get; set; }

        [JsonProperty("TwilioToken")]
        public string TwilioToken { get; set; }

        [JsonProperty("TwilioPhoneNumber")]
        public string TwilioPhoneNumber { get; set; }
    }
}
