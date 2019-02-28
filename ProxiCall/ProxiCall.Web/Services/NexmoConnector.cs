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

            byte[] audioBytes;

            var silence = new byte[32000];
            var silence_buffer = new ArraySegment<byte>(silence);
            var lenght=0;

            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                //audioBytes = await TextToSpeech.TransformTextToSpeechAsync("No No No No No No No No No.", "en-US");
                //var audioBuffer = new ArraySegment<byte>(audioBytes, 0, audioBytes.Length);

                //Audio
                //await webSocket.SendAsync(audioBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);

                //Silence
                //await webSocket.SendAsync(silence_buffer, WebSocketMessageType.Binary, true, CancellationToken.None);

                //Buffer
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
