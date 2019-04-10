using System.Collections.Generic;

namespace ProxiCall.CRM.Models
{
    public class LoginDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
        public string Token { get; set; }
    }
}
