using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProxiCall.Web.Models
{
    public class OutlookCalendarResult
    {
        public OutlookCalendarResult()
        {

        }

        private List<OutlookCalendarEvent> outlookCalendarEventValue;

        [JsonProperty(PropertyName = "value")]
        public List<OutlookCalendarEvent> OutlookCalendarEventValue
        {
            get { return outlookCalendarEventValue; }
            set { outlookCalendarEventValue = value; }
        }

    }
}
