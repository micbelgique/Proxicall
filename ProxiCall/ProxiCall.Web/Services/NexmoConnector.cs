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
            byte[] sendBuffer;
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                sendBuffer = await TextToSpeech.TransformTextToSpeechAsync("I hate everything, especially php", "en-US");
                //sendBuffer = buffer;
                
                await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
