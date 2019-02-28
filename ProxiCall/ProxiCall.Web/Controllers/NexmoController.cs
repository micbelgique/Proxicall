using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ProxiCall.Web.Services;
using ProxiCall.Web.Services.Speech;
using System.Threading.Tasks;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NexmoController : ControllerBase
    {
        private BotConnector _botConnector;

        private static async Task<string> TestSpeech()
        {
            byte[] result_tts = await TextToSpeech.TransformTextToSpeechAsync("This is a test in english.", "en-US");
            var str = await SpeechToText.RecognizeSpeechFromBytesAsync(result_tts, "en-US");
            return str;
        }

        [HttpGet("answer")]
        public IActionResult AnswerHandler()
        {
            //const string host = "bfa64ee2.ngrok.io";
            const string host = "proxicallmel.azurewebsites.net";

            //_botConnector = new BotConnector();
            var nccos = new JArray();

            //var result = TestSpeech().Result;
            var result = "Test Talk";

            var nccoTalk = new JObject()
            {
                { "action", "talk" },
                { "text", result }
            };

            var nccoConnect = new JObject()
            {
                { "action", "connect" },
                { "endpoint", new JArray(new JObject{
                        { "type", "websocket" },
                        { "uri", $"wss://{host}/socket"},
                        { "content-type", "audio/l16;rate=16000"},
                        { "headers",  new JObject {
                                //{ "conversationID", _botConnector.ConversationId }
                                { "conversationID", "ifhjvbfhvbahfbd" }
                            }
                        }
                    })
                }
            };
            nccos.Add(nccoTalk);
            nccos.Add(nccoConnect);
            return Content(nccos.ToString(), "application/json");
        }
        
        [HttpPost("event")]
        public IActionResult EventHandler()
        {
            return Ok();
        }
    }
}