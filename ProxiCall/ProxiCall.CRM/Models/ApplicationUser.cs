using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ProxiCall.Library;

namespace ProxiCall.CRM.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Alias { get; set; }

        [Required]
        public override string PhoneNumber { get; set; }

        private string language;

        [Display(Name = "Language of choice")]
        public string Language
        {
            get
            {
                if(string.IsNullOrEmpty(language))
                {
                    language = LanguagesManager.DEFAULT;
                }
                return language;
            }
            set { language = value; }
        }


        public ApplicationUser()
        {
            Language = LanguagesManager.DEFAULT;
        }
    }
}
