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

        public delegate Task OnReplyHandler(IList<Activity> botReplies, string callSid);

        public BotConnector(string callSid)
        {
            _directLineClient = new DirectLineClient(Environment.GetEnvironmentVariable("DirectLineSecret"));
            var conversation = _directLineClient.Conversations.StartConversation();
            _conversationId = conversation.ConversationId;
            _streamUrl = conversation.StreamUrl;
            _callSid = callSid;
        }

        public async Task ReceiveMessagesFromBotAsync(OnReplyHandler SendActivitiesToUser)
        {
            var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(_streamUrl), CancellationToken.None);


            string botReply = String.Empty;
            var replyBuffer = ClientWebSocket.CreateClientBuffer(1024 * 4, 1024 * 4);
            
            var activities = new List<Activity>();

            WebSocketReceiveResult websocketReceivedResult = new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
            while (!websocketReceivedResult.CloseStatus.HasValue)
            {
                do
                {
                    websocketReceivedResult = await webSocket.ReceiveAsync(replyBuffer, CancellationToken.None);
                } while (!websocketReceivedResult.EndOfMessage);

                if (websocketReceivedResult.Count != 0)
                {
                    botReply = Encoding.UTF8.GetString(replyBuffer.ToArray(), 0, websocketReceivedResult.Count);
                    var activitySet = JsonConvert.DeserializeObject<ActivitySet>(botReply);
                    var isFromBot = true;
                    foreach (Activity activity in activitySet.Activities)
                    {
                        isFromBot = activity.From.Name == "ProxiCallBot";
                        if (isFromBot)
                        {
                            activities.Add(activity);
                            if(activity.InputHint != InputHints.IgnoringInput)
                            {
                                await SendActivitiesToUser(activities, _callSid);
                                activities.Clear();
                            }
                        }
                    }
                }
            }
            await webSocket.CloseAsync(websocketReceivedResult.CloseStatus.Value, websocketReceivedResult.CloseStatusDescription, CancellationToken.None);
        }

        public async Task SendMessageToBotAsync(Activity userMessage)
        {
            await _directLineClient.Conversations.PostActivityAsync(_conversationId, userMessage, CancellationToken.None);
        }
    }
}
