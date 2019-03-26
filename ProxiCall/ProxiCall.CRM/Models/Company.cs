using System.Collections.Generic;
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
        [Display(Name="Reference lead")]
        public string RefLeadId { get; set; }
        [ForeignKey("RefLeadId")]
        public Lead RefLead { get; set; }
        [NotMapped]
        public ICollection<Lead> Leads { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
