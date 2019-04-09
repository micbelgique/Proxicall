using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proxicall.CRM.Models
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
