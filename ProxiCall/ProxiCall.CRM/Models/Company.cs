using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxiCall.CRM.Models
{
    public class Company
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        
        [Display(Name = "Contact")]
        [ForeignKey("Contact")]
        public string ContactId { get; set; }
        //[Display(Name = "Contact")]
        public Lead Contact { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
