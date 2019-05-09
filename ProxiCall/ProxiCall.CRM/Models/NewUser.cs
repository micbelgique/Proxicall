using System.ComponentModel.DataAnnotations;

namespace ProxiCall.CRM.Models
{
    public class NewUser
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Alias")]
        public string Alias { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Administrator")]
        public bool IsAdmin { get; set; } = false;

        [Display(Name = "Language of choice")]
        public string Language { get; set; }
    }
}
