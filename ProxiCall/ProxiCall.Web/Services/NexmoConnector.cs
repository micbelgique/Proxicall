using Microsoft.AspNetCore.Http;
using System;
using System.Web;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ProxiCall.Web.Services.Speech;
using Microsoft.Extensions.Logging;
using System.Text;
using Nexmo.Api;
using Newtonsoft.Json;
using ProxiCall.Web.Models.DTO;
using Nexmo.Api.Voice;

namespace ProxiCall.Web.Services
{
    public class NexmoConnector
    {
        public static ILogger<Startup> Logger { get; set; }

        public static async Task NexmoSpeechToText(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                while(!result.EndOfMessage)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                var text = SpeechToText.RecognizeSpeechFromBytesAsync(buffer).Result;
                Console.WriteLine(text);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public static async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            //var firstMessage = Encoding.UTF8.GetString(buffer);
            //var dto = JsonConvert.DeserializeObject<NexmoFirstMessageDTO>(firstMessage);
            
            //Logger.LogInformation($"UUID IN WEBSOCKET {dto.Uuid}");

            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public static async Task TestPlayAudio(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var firstMessage = Encoding.UTF8.GetString(buffer);
            var dto = JsonConvert.DeserializeObject<NexmoFirstMessageDTO>(firstMessage);

            Logger.LogInformation($"UPDATE UUID IN WEBSOCKET {dto.Uuid}");
            Logger.LogInformation($"API KEY : {Configuration.Instance.Settings["appsettings:Nexmo.api_key"]}");

            Logger.LogInformation($"APP KEY : {Configuration.Instance.Settings["appsettings:Nexmo.Application.Key"]}");

            var _client = new Client(creds: new Nexmo.Api.Request.Credentials
            {
                ApiKey = Configuration.Instance.Settings["appsettings:Nexmo.api_key"],
                ApiSecret = Configuration.Instance.Settings["appsettings:Nexmo.api_secret"],
                ApplicationId = Configuration.Instance.Settings["appsettings:Nexmo.Application.Id"],
                ApplicationKey = Configuration.Instance.Settings["appsettings:Nexmo.Application.Key"]
            });
            //Logger.LogInformation($"CLIENT 2 : {_client.Credentials.ApiKey} ");
            //Logger.LogInformation($"CLIENT 3 : {_client.Credentials.ApiSecret} ");
            //Logger.LogInformation($"CLIENT 4 : {_client.Credentials.ApplicationId} ");
            //Logger.LogInformation($"CLIENT 5 : {_client.Credentials.ApplicationKey} ");
            //if (_client==null)
            //Logger.LogInformation($"CLIENT NULL ");
            //var command = new Nexmo.Api.Voice.Call.StreamCommand();
            //command.stream_url = new string[]
            //    {
            //        "http://www.noiseaddicts.com/samples_1w72b820/201.mp3"
            //    };
            //Logger.LogInformation($"COMMAND : {command.stream_url[0]} ");
            //_client.Call.BeginStream(dto.Uuid, command);
            var test = _client.Call.BeginTalk(dto.Uuid, new Call.TalkCommand
            {
                text = "Hello Text to Speech",
                voice_name = "Kimberly"
            });
        }

        public static async Task NexmoTextToSpeech(HttpContext context, WebSocket webSocket)
        {
            var ttsAudio = await TextToSpeech.TransformTextToSpeechAsync("This is a test", "en-US");
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            Logger.LogInformation($"First message : {Encoding.UTF8.GetString(buffer)}");
            while (!result.CloseStatus.HasValue)
            {
                Logger.LogInformation("In NexmoTextToSpeech ws loop");
                await SendSpeech(context, webSocket, ttsAudio);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing Socket", CancellationToken.None);
        }

        private static async Task SendSpeech(HttpContext context, WebSocket webSocket, byte[] ttsAudio)
        {
            const int chunkSize = 640;
            var chunkCount = 1;
            var offset = 0;
            
            var lastFullChunck = ttsAudio.Length < (offset + chunkSize);

            Logger.LogInformation($"ttsAudio length : {ttsAudio.Length}");
            Logger.LogInformation($"lastFullChunck : {lastFullChunck}");
            try
            {
                while(!lastFullChunck)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset, chunkSize), WebSocketMessageType.Binary, false, CancellationToken.None);
                    Logger.LogInformation($"SendSpeech loop offset : {offset}");
                    offset = chunkSize * chunkCount;
                    lastFullChunck = ttsAudio.Length < (offset + chunkSize);
                    chunkCount++;
                }

                Logger.LogInformation($"Offset after loop : {offset}");

                var lastMessageSize = ttsAudio.Length - offset;
                Logger.LogInformation($"SendSpeech after loop lastMessageSize : {lastMessageSize}");
                await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset, lastMessageSize), WebSocketMessageType.Binary, true, CancellationToken.None);

                Logger.LogInformation($"Number of bytes sent : {offset+lastMessageSize}");
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Exception while sending spliced ttsAudio : {ex.Message}");
            }
        }
    }
}
