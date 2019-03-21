using Proxicall.CRM.Models.Enumeration.Opportunity;
using ProxiCall.CRM.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Proxicall.CRM.Models
{
    public class Opportunity
    {
        [Key]
        public string Id { get; set; }

        //[Display(Name = "Owner")]
        //public string OwnerId { get; set; }
        //public Account Owner { get; set; }

        [Display(Name = "Lead")]
        public string LeadId { get; set; }
        public Lead Lead { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime EstimatedCloseDate { get; set; }
        public string Comments { get; set; }
        public Status Status { get; set; }
        public Confidence Confidence { get; set; }
    }
}
