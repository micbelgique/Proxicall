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
        [ForeignKey("RefLead")]
        public string RefLeadId { get; set; }
        public Lead RefLead { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
