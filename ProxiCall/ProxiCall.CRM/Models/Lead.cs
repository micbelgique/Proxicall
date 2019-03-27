using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxiCall.CRM.Models
{
    //Lead <> User
    public class Lead
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        [ForeignKey("Employer")]
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Company")]
        [JsonIgnore]
        public Company Employer { get; set; }    
        
        [NotMapped]
        [Display(Name = "Full name")]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
