using Console_Speech.Services.Speech;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json.Linq;
using Nexmo.Api;
using Nexmo.Api.Voice;
using ProxiCall.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Web.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class VoiceControllerOld : Controller
    {
        private BotConnector _botConnector;
        private NexmoConnector _nexmoConnector;
        

        [HttpGet("answer")]
        public IActionResult GetAnswer()
        {
            var ncco = new JArray();

            return Content("This is a test");
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

            foreach(var activity in botReplies)
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