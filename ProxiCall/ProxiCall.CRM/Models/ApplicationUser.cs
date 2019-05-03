using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ProxiCall.CRM.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Alias { get; set; }

        [Required]
        public override string PhoneNumber { get; set; }
    }
}
