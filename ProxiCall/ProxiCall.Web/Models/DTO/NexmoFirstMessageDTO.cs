using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Web.Models.DTO
{
    public class NexmoFirstMessageDTO
    {
        public NexmoFirstMessageDTO(string conversationId, string uuid, string eventType, string contentType)
        {
            ConversationId = conversationId;
            Uuid = uuid;
            EventType = eventType;
            ContentType = contentType;
        }

        public string ConversationId { get; }
        public string Uuid { get; }
        [JsonProperty(PropertyName = "event")]
        public string EventType { get; }
        [JsonProperty(PropertyName = "content-type")]
        public string ContentType { get; }
    }
}
