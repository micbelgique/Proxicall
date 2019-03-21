using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProxiCall.CRM.Models
{
    public class Company
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }

        public ICollection<Lead> Leads { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
