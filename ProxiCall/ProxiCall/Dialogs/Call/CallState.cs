using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.Call
{
    public class CallState
    {
        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
        public string PhoneNumber { get; set; }

        private string fullName;

        public string RecipientFullName {
            get { return fullName; }
            set {
                fullName = value;
                var names = fullName.Split(new char[0]); //split at each whitespaces
                var firstName = names[0];
                var lastName = names[1];
                RecipientFirstName = firstName;
                RecipientLastName = lastName;
            }
        }

    }
}
