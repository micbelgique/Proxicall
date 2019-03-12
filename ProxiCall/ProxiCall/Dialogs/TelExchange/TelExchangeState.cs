using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.TelExchange
{
    public class TelExchangeState
    {
        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
        public string PhoneNumber { get; set; }

        private string recipientFullname;

        public string RecipientFullName {
            get { return recipientFullname; }
            set {
                recipientFullname = value;

                var names = recipientFullname.Split(new char[0]); //split at each whitespaces
                var firstName = names[0];
                var lastName = new StringBuilder();

                for(var i = 1; i < names.Length; i++)
                {
                    lastName.Append(names[i] + " ");
                }

                RecipientFirstName = firstName;
                RecipientLastName = lastName.ToString();
            }
        }

        public string IntentName { get; set; }
    }
}
