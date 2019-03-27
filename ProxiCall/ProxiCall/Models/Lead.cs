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
        public string Company { get; set; }

        public Lead()
        {
            Reset();
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
                    LastName = lastName.ToString();
                }
                else
                {
                    FirstName = string.Empty;
                    LastName = string.Empty;
                }
            }
        }

        public void Reset()
        {
            Id = string.Empty;
            FullName = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            Company = string.Empty;
        }
    }
}
