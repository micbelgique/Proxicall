using System.Text;

namespace ProxiCall.Models
{
    public class Lead
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public Company Company { get; set; }

        public Lead()
        {

        }
        
        //TODO : include company to response from http request in LeadService
        public static Lead CloneWithCompany(Lead lead, Company company)
        {
            Lead newLead = new Lead
            {
                Id = lead.Id,
                FullName = lead.FullName,
                Email = lead.Email,
                PhoneNumber = lead.PhoneNumber,
                Address = lead.Address,
                Company = company
            };

            return newLead;
        }

        public Lead(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        private string fullname = string.Empty;

        public string FullName
        {
            get { return fullname; }
            set
            {
                fullname = value;

                if (!string.IsNullOrEmpty(value))
                {
                    var names = fullname.Split(new char[0]); //split at each whitespaces
                    var firstName = names[0];
                    var lastName = new StringBuilder();

                    for (var i = 1; i < names.Length; i++)
                    {
                        lastName.Append(names[i] + " ");
                    }
                    
                    FirstName = firstName;
                    LastName = lastName.ToString().Trim();
                }
                else
                {
                    FirstName = string.Empty;
                    LastName = string.Empty;
                }
            }
        }
    }
}
