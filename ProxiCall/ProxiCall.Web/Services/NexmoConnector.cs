using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ProxiCall.Web.Services.Speech;
using Microsoft.Extensions.Logging;
using System.Text;
using Nexmo.Api;
using Newtonsoft.Json;
using ProxiCall.Web.Models.DTO;

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

            var firstMessage = Encoding.UTF8.GetString(buffer);
            var dto = JsonConvert.DeserializeObject<NexmoFirstMessageDTO>(firstMessage);
            
            Logger.LogInformation($"UUID IN WEBSOCKET {dto.Uuid}");
            TestPlayAudio(dto.Uuid);

            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private static void TestPlayAudio(string uuid)
        {
            Logger.LogInformation($"appsettings test : {Configuration.Instance.Settings["appsettings:Nexmo.api_key"]}");
            var _client = new Client(creds: new Nexmo.Api.Request.Credentials
            {
                ApiKey = Configuration.Instance.Settings["appsettings:Nexmo.api_key"],
                ApiSecret = Configuration.Instance.Settings["appsettings:Nexmo.api_secret"],
                ApplicationId = Configuration.Instance.Settings["appsettings:Nexmo.Application.Id"],
                ApplicationKey = Configuration.Instance.Settings["appsettings:Nexmo.Application.Key"]
            });
            _client.Call.BeginStream(uuid, new Nexmo.Api.Voice.Call.StreamCommand
            {
                stream_url = new[]
                {
                    "https://raw.githubusercontent.com/nexmo-community/ncco-examples/gh-pages/assets/welcome_to_nexmo.mp3"
                }
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
