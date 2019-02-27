using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ProxiCall.Web.Services;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NexmoController : ControllerBase
    {
        private BotConnector _botConnector;

        [HttpGet("answer")]
        public IActionResult AnswerHandler()
        {
            const string host = "proxicallhub.azurewebsites.net";

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
                                { "conversationID", "dghjnxfghjhgdjk" }
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