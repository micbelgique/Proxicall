using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Nexmo.Api;
using ProxiCall.Web.Models;
using ProxiCall.Web.Services;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NexmoController : ControllerBase
    {
        private BotConnector _botConnector;
        private readonly Client _client;

        public NexmoController(ILogger<NexmoController> logger)
        {
            Logger = logger;
            _client = new Client(creds: new Nexmo.Api.Request.Credentials
            {
                ApiKey = Configuration.Instance.Settings["appsettings:Nexmo.api_key"],
                ApiSecret = Configuration.Instance.Settings["appsettings:Nexmo.api_secret"]
            });
        }

        public ILogger<NexmoController> Logger { get; }

        [HttpGet("answer")]
        public IActionResult AnswerHandler([FromQuery] string uuid)
        {
            string host = Configuration.Instance.Settings["azure:host"];

            //_botConnector = new BotConnector();

            var nccoWS = new JArray(new JObject()
            {
                { "action", "connect" },
                { "endpoint", new JArray(new JObject{
                        { "type", "websocket" },
                        { "uri", $"wss://{host}/socket"},
                        { "content-type", "audio/l16;rate=16000"},
                        { "headers",  new JObject {
                                //{ "conversationID", _botConnector.ConversationId }
                                { "conversationID", "dghjnxfghjhgdjk" },
                                { "uuid", uuid }
                            }
                        }
                    })
                }
            });

            return Content(nccoWS.ToString(), "application/json");
        }
        
        [HttpPost("event")]
        public IActionResult EventHandler()
        {
            return Ok();
        }
    }
}