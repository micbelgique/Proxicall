namespace ProxiCall.Bot.Models
{
    public class Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        
        public Lead Contact { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
