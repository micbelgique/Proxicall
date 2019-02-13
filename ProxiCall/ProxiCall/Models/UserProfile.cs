namespace ProxiCall.Models
{
    public class UserProfile
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string phoneNumber;

        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { phoneNumber = value; }
        }


        public UserProfile(string name = "NA", string phoneNumber = "NA")
        {
            Name = name;
        }

        public UserProfile()
        {
        }

        public override string ToString()
        {
            return base.ToString() + "Name: " + name + "\tPhone Number" + phoneNumber;
        }
    }
}
