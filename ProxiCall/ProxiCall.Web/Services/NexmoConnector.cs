using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ProxiCall.Web.Services.Speech;

namespace ProxiCall.Web.Services
{
    public class NexmoConnector
    {
        public static async Task Echo(HttpContext context, WebSocket webSocket)
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

        private static async Task SendSpeech(HttpContext context, WebSocket webSocket)
        {
            var ttsAudio = await TextToSpeech.TransformTextToSpeechAsync("I hate everything, especially php", "en-US");
            var chunkSize = 640;
            var chunkCount = 1;
            var complete = false;
            var offset = 0;

            var lastChunkIndex = ttsAudio.Length / chunkSize;

            try
            {
                while (!complete)
                {
                    offset = chunkSize * chunkCount;
                    if (chunkCount != lastChunkIndex)
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset - 1, chunkSize), WebSocketMessageType.Binary, complete, CancellationToken.None);
                    }
                    else
                    {
                        complete = true;
                        await webSocket.SendAsync(new ArraySegment<byte>(ttsAudio, offset - 1, ttsAudio.Length - offset), WebSocketMessageType.Binary, complete, CancellationToken.None);
                    }
                    chunkCount++;
                }
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            
        }
    }
}
