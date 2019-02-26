using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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
        private WebSocket _webSocket;

        [HttpGet("answer")]
        public IActionResult AnswerHandler()
        {
            /* Nexmo tts for testing connection
            var nccos = new JArray();
            var nccoTalk = new JObject();
            nccoTalk.Add("action", "talk");
            nccoTalk.Add("text", "You are listening to a test text-to-speech call made with Nexmo Voice API");
            */
            //const string host = "proxicallweb.azurewebsites.net";
            const string host = "07ab57b8.ngrok.io";

            var nccoWS = new JArray(new JObject()
            {
                { "action", "connect" },
                { "endpoint", new JArray(new JObject{
                        { "type", "websocket" },
                        { "uri", $"wss://{host}/ws"},
                        { "content-type", "audio/l16;rate=16000"},
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

        [HttpGet("socket")]
        public async Task GetAudio()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (_webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    await Echo(HttpContext, _webSocket);
                }
                //_botConnector = new BotConnector();

                //_botConnector.ReceiveMessagesFromBotAsync(OnBotReplyHandler);

                /*var receivingBuffer = new byte[1024 * 4];
                var result = new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true);
                while (!result.CloseStatus.HasValue)
                {
                    do
                    {
                        result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivingBuffer), CancellationToken.None);
                    } while (!result.EndOfMessage);
                    
                    var audioReceived = receivingBuffer.ToArray();
                    await OnAudioReceivedAsync(audioReceived);
                }
                await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);*/
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var test = "Hello ! I hate you.";
            var buffer = await TextToSpeech.TransformTextToSpeechAsync(test, "fr-FR");
            //var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var result = new WebSocketReceiveResult(1024, WebSocketMessageType.Text, true);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Count()), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async void OnBotReplyHandler(IList<Activity> botReplies)
        {
            var replyNeeded = botReplies.Last().InputHint != "ignoringInput";

            foreach (var activity in botReplies)
            {
                var audioToSend = await TextToSpeech.TransformTextToSpeechAsync(activity.Text, "fr-FR");
                await SendAudioAsync(audioToSend);
            }
        }

        public async Task SendAudioAsync(byte[] audioToSend)
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(audioToSend, 0, audioToSend.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
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