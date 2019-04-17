
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProxiCall.Web.Models
{
    public class OutlookCalendarEvent
    {
        public OutlookCalendarEvent()
        {

        }

        public OutlookCalendarEvent(string subject, string bodyPreview, string dateStart, string dateEnd, string body, string location, string organizerName = "", string organizerEmail = "")
        {
            Subject = subject;
            BodyPreview = bodyPreview;
            Start = new EventTime(dateStart);
            End = new EventTime(dateEnd);
            Body = new Body(body);
            Location = new Location(location);
            Organizer = new Organizer(new EmailAddress(organizerEmail, organizerName));
            Attendees = new List<Attendee>();
        }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "bodyPreview")]
        public string BodyPreview { get; set; }
        [JsonProperty(PropertyName = "organizer")]
        public Organizer Organizer { get; set; }
        [JsonProperty(PropertyName = "body")]
        public Body Body { get; set; }
        [JsonProperty(PropertyName = "location")]
        public Location Location { get; set; }
        [JsonProperty(PropertyName = "attendees")]
        public List<Attendee> Attendees { get; set; }
        [JsonProperty(PropertyName = "start")]
        public EventTime Start { get; set; }
        [JsonProperty(PropertyName = "end")]
        public EventTime End { get; set; }
        [JsonProperty(PropertyName = "reminderMinutesBeforeStart")]
        public int ReminderMinutesBeforeStart { get; set; }
        [JsonProperty(PropertyName = "isReminderOn")]
        public bool IsReminderOn { get; set; }
        [JsonProperty(PropertyName = "isOrganizer")]
        public bool IsOrganizer { get; set; } = false;
    }

    public class EventTime
    {
        public EventTime(string dateTime)
        {
            TimeZone = "UTC";
            DateTime = dateTime;
        }


        [JsonProperty(PropertyName = "dateTime")]
        public string DateTime { get; set; }
        [JsonProperty(PropertyName = "timeZone")]
        public string TimeZone { get; set; }
    }

    public class Location
    {
        public Location(string displayName)
        {
            DisplayName = displayName;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
        [JsonProperty(PropertyName = "locationEmailAddress")]
        public string LocationEmailAddress { get; set; }
    }

    public class Attendee
    {
        public Attendee(EmailAddress emailAddress, string type = "required")
        {
            EmailAddress = emailAddress;
            Type = type;
        }

        [JsonProperty(PropertyName = "emailAddress")]
        public EmailAddress EmailAddress { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class Body
    {
        public Body(string content)
        {
            Content = content;
        }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }

    public class Organizer
    {
        public Organizer(EmailAddress emailAddress)
        {

            EmailAddress = emailAddress;
        }

        [JsonProperty(PropertyName = "emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }

    public class EmailAddress
    {
        public EmailAddress(string address, string name)
        {
            Address = address;
            Name = name;
        }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
