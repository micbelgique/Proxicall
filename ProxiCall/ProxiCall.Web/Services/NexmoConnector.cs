using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ProxiCall.Web.Services.Speech;
using Microsoft.Extensions.Logging;

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
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public static async Task NexmoTextToSpeech(HttpContext context, WebSocket webSocket)
        {
            var ttsAudio = await TextToSpeech.TransformTextToSpeechAsync("This is a test", "en-US");
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            Logger.LogInformation($"First message : {buffer.ToString()}");

            while (!result.CloseStatus.HasValue)
            {
                Logger.LogInformation("In NexmoTextToSpeech ws loop");
                await SendSpeech(context, webSocket, ttsAudio);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
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
                while(lastFullChunck)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset, chunkSize), WebSocketMessageType.Binary, false, CancellationToken.None);
                    Logger.LogInformation($"SendSpeech loop offset : {offset}");
                    chunkCount++;
                    offset = chunkSize * chunkCount - 1;
                    lastFullChunck = ttsAudio.Length < (offset + chunkSize);
                }
                var lastMessageSize = ttsAudio.Length - offset;
                Logger.LogInformation($"SendSpeech after loop lastMessageSize : {lastMessageSize}");
                await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset, lastMessageSize), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            
        }
    }
}
