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
            var isDone = false;
            var ttsAudio = await TextToSpeech.TransformTextToSpeechAsync("Dear Websocket and Nexmo, just work. Thank you.", "en-US");
            var buffer = new byte[1024 * 6];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            Logger.LogInformation($"First message : {buffer.ToString()}");

            while (!result.CloseStatus.HasValue)
            {
                if (!isDone)
                {
                    Logger.LogInformation("In NexmoTextToSpeech WS ISDONE loop");
                    isDone = await SendSpeech(context, webSocket, ttsAudio);
                }
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            Logger.LogInformation($"AFTER WS LOOP : Closing");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing Socket", CancellationToken.None);
        }

        private static async Task<bool> SendSpeech(HttpContext context, WebSocket webSocket, byte[] ttsAudio)
        {
            const int chunkSize = 640;
            var chunkCount = 1;
            var offset = 0;
            
            var lastFullChunck = ttsAudio.Length < (offset + chunkSize);

            Logger.LogInformation($"ttsAudio length : {ttsAudio.Length} | lastFullChunck : {lastFullChunck}");
            try
            {
                while(!lastFullChunck)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset, chunkSize), WebSocketMessageType.Binary, false, CancellationToken.None);
                    Logger.LogInformation($"SendSpeech IN loop offset : {offset}");
                    offset = chunkSize * chunkCount;
                    chunkCount++;
                    lastFullChunck = ttsAudio.Length < (offset + chunkSize);
                }
                var lastMessageSize = ttsAudio.Length - offset;
                Logger.LogInformation($"SendSpeech AFTER loop offset : {offset} | lastMessageSize : {lastMessageSize}");
                await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset, lastMessageSize), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return true;
        }
    }
}
