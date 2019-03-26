using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.Shared
{
    public class LeadState
    {
        public string LeadFirstName { get; set; }
        public string LeadLastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }

        private string leadFullname = string.Empty;

        public string LeadFullName
        {
            get { return leadFullname; }
            set
            {
                leadFullname = value;

                var names = leadFullname.Split(new char[0]); //split at each whitespaces
                var firstName = names[0];
                var lastName = new StringBuilder();

                for (var i = 1; i < names.Length; i++)
                {
                    lastName.Append(names[i] + " ");
                }

                LeadFirstName = firstName;
                LeadLastName = lastName.ToString();
            }
        }

        public void Reset()
        {
            LeadFirstName = string.Empty;
            PhoneNumber = string.Empty;
            Address = string.Empty;
            Company = string.Empty;
        }
    }
}
