using Microsoft.AspNetCore.Mvc;
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
        private readonly string _conversationId;
        private readonly string _streamUrl;
        private readonly string _callSid;

        public delegate void OnReplyHandler(IList<Activity> botReplies, string callSid);

        public BotConnector(string callSid)
        {
            var dlSecret = Environment.GetEnvironmentVariable("DirectLineSecret");
            if(dlSecret == "") {
                throw new Exception("DirectLineSecret doesn't exist");
            }
            _directLineClient = new DirectLineClient(dlSecret);
            var conversation = _directLineClient.Conversations.StartConversation();
            _conversationId = conversation.ConversationId;
            _streamUrl = conversation.StreamUrl;
            _callSid = callSid;
        }

        public async Task ReceiveMessagesFromBotAsync(OnReplyHandler SendActivitiesToUser)
        {
            var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(_streamUrl), CancellationToken.None); 

            var buffer = ClientWebSocket.CreateClientBuffer(1024 * 4, 1024 * 4);

            string repliesFromBot = String.Empty;
            WebSocketReceiveResult websocketReceivedResult = new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
            while (!websocketReceivedResult.CloseStatus.HasValue)
            {
                do
                {
                    websocketReceivedResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                } while (!websocketReceivedResult.EndOfMessage);

                if (websocketReceivedResult.Count != 0)
                {
                    repliesFromBot = Encoding.UTF8.GetString(buffer.ToArray(), 0, websocketReceivedResult.Count);
                    var activitySet = JsonConvert.DeserializeObject<ActivitySet>(repliesFromBot);
                    var activities = new List<Activity>();
                    foreach (Activity activity in activitySet.Activities)
                    {
                        if (activity.From.Name == "ProxiCallBot")
                        {
                            activities.Add(activity);
                        }
                    }
                    SendActivitiesToUser(activities, _callSid);
                }
            }
            await webSocket.CloseAsync(websocketReceivedResult.CloseStatus.Value, websocketReceivedResult.CloseStatusDescription, CancellationToken.None);
        }

        public async Task SendMessageToBotAsync(Activity userMessage)
        {
            var postedActivity = await _directLineClient.Conversations.PostActivityAsync(_conversationId, userMessage, CancellationToken.None);
            Console.WriteLine(postedActivity);
        }
    }
}
