using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Web.Services
{
    public class NexmoConnector
    {
        public delegate Task OnAudioReceivedHandler(byte[] audioReceived);

        private WebSocket _webSocket;

        public NexmoConnector(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public async Task WebsocketHandler(OnAudioReceivedHandler audioReceivedHandler)
        {
            var receivingBuffer = WebSocket.CreateServerBuffer(1024 * 4);
            var result = new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true);
            while (!result.CloseStatus.HasValue)
            {
                do
                {
                    result = await _webSocket.ReceiveAsync(receivingBuffer, CancellationToken.None);
                } while (!result.EndOfMessage);
                var audioReceived = receivingBuffer.ToArray();
                await audioReceivedHandler(audioReceived);
            }
            await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task SendAudioAsync(byte[] audioToSend)
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(audioToSend, 0, audioToSend.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }
}
