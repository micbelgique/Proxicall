using Nancy;
using Newtonsoft.Json.Linq;
using System.Net;

namespace NexmoVoiceASPNetCoreQuickStarts
{
    public class VoiceModule : NancyModule 
    {
        public VoiceModule()
        {
            /// <summary>
            /// Depending on what you want to achieve (inbound call, handle DTMF input etc...)
            /// pick the suitable method to return the right NCCO for webhook/answer
            /// </summary>
            Get["/webhook/answer"] = x => GetInboundNCCO();
            //Get["/webhook/answer"] = x => "Hello happy path";
            Post["/webhook/dtmf"] = x => GetDTMFInput();
            Post["/webhook/event"] = x => Request.Query["status"];
        }

        private string GetInboundNCCO()
        {
            dynamic TalkNCCO = new JObject();
            TalkNCCO.action = "talk";
            var from = this.Request.Query["from"];
            TalkNCCO.text = "Thank you for calling from "+ string.Join(" ", from);
            TalkNCCO.voiceName = "Amy";
            //TEST TalkNCCO.text = "<speak><lang xml:lang='pt-BR'>Bom dia.</lang> <prosody rate='fast'>I can speak fast.</prosody> <lang xml:lang='fr'>Au revoir!</lang></speak>";

            JArray jarrayObj = new JArray();
            jarrayObj.Add(TalkNCCO);
            var json = jarrayObj.ToString();
            return json;

        }

        private string GetDTMFNCCO()
        {
            dynamic TalkNCCO = new JObject();
            TalkNCCO.action = "talk";
            TalkNCCO.text = "Hello. Please press any key to continue.";

            JArray jarrayObj = new JArray();
            jarrayObj.Add(TalkNCCO);

            dynamic InputNCCO = new JObject();
            InputNCCO.action = "input";
            InputNCCO.maxDigits = "1";
            InputNCCO.eventUrl = $"{Request.Url.SiteBase}/webhook/dtmf";

            jarrayObj.Add(InputNCCO);

            return jarrayObj.ToString();
        }

        private string GetDTMFInput()
        {
            dynamic TalkNCCO = new JObject();
            TalkNCCO.action = "talk";
            TalkNCCO.text = $"You pressed {Request.Query["dtmf"]} ";

            JArray jarrayObj = new JArray();
            jarrayObj.Add(TalkNCCO);

            return jarrayObj.ToString();

        }
    }
}
