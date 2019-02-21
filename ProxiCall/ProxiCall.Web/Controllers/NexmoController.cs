using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Console_Speech.Services.Speech;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json.Linq;
using ProxiCall.Web.Services;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NexmoController : ControllerBase
    {
        private BotConnector _botConnector;
        private NexmoConnector _nexmoConnector;

        [HttpGet("answer")]
        public IActionResult AnswerHandler()
        {
            var nccos = new JArray();

            var nccoTalk = new JObject();
            nccoTalk.Add("action", "talk");
            nccoTalk.Add("text", "You are listening to a test text-to-speech call made with Nexmo Voice API");

            nccos.Add(nccoTalk);
            return Content(nccos.ToString(), "application/json");
        }
        
        [HttpPost("event")]
        public IActionResult EventHandler()
        {
            return Ok();
        }

        [HttpGet("ws")]
        public async Task GetAudio()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _botConnector = new BotConnector();
                _nexmoConnector = new NexmoConnector(webSocket);

                await _botConnector.ReceiveMessagesFromBotAsync(OnBotReplyHandler);
                await _nexmoConnector.WebsocketHandler(OnAudioReceivedAsync);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        public async void OnBotReplyHandler(IList<Activity> botReplies)
        {
            var replyNeeded = botReplies.Last().InputHint != "ignoringInput";

            foreach (var activity in botReplies)
            {
                var audioToSend = await TextToSpeech.TransformTextToSpeechAsync(activity.Text, "fr-FR");
                await _nexmoConnector.SendAudioAsync(audioToSend);
            }
        }

        public async Task OnAudioReceivedAsync(byte[] audioReceived)
        {
            var textFromUser = await SpeechToText.RecognizeSpeechFromBytesAsync(audioReceived, "fr-FR");
            var activity = new Activity();
            activity.From = new ChannelAccount("userid", "username"); //TODO replace id and name with userid and username from nexmo
            activity.Type = "message";
            activity.Text = textFromUser;
            await _botConnector.SendMessageToBotAsync(activity);
        }
    }
}