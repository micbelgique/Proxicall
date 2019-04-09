using System.Collections.Generic;

namespace ProxiCall.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Alias { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
        public string Token { get; set; }
    }
}
