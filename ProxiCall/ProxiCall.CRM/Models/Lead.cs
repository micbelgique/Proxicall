using System.ComponentModel.DataAnnotations;

namespace ProxiCall.CRM.Models
{
    //Lead <> User
    public class Lead
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        public Company Company { get; set; }      
    }
}
