using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxiCall.Web.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string HomeAddress { get; set; }

        public string ToString()
        {
            var user = new StringBuilder();
            user.Append(FirstName);
            user.Append(" ");
            user.Append(LastName);

            return user.ToString();
        }
    }
}
