using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Web.Services
{
    public class BotConnector
    {
        private DirectLineClient _directLineClient;
        private string _conversationId;
        private string _streamUrl;

        public delegate Activity OnReplyHandler(IList<Activity> botReply);

        public BotConnector()
        {
            _directLineClient = new DirectLineClient(Environment.GetEnvironmentVariable("DirectLineSecret"));
            var conversation = _directLineClient.Conversations.StartConversation();
            _conversationId = conversation.ConversationId;
            _streamUrl = conversation.StreamUrl;
        }

        public async Task StartWebsocket(OnReplyHandler SendToUser)
        {
            var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(_streamUrl), CancellationToken.None); 

            var buffer = ClientWebSocket.CreateClientBuffer(1024 * 4, 1024 * 4);

            string reply;
            WebSocketReceiveResult result = new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
            while (!result.CloseStatus.HasValue)
            {
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                } while (!result.EndOfMessage);
                if (result.Count == 0 && result.EndOfMessage)
                {
                    reply = String.Empty; //ignore empty message
                }
                else
                {
                    reply = Encoding.UTF8.GetString(buffer.ToArray(), 0, result.Count);
                    var activitySet = JsonConvert.DeserializeObject<ActivitySet>(reply);
                    var activities = new List<Activity>();
                    foreach (Activity activity in activitySet.Activities)
                    {
                        if (activity.From.Name == "ProxiCallBot")
                        {
                            activities.Add(activity);
                        }
                    }
                    var userReply = SendToUser(activities);
                    await _directLineClient.Conversations.PostActivityAsync(_conversationId, userReply, CancellationToken.None);
                }
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
