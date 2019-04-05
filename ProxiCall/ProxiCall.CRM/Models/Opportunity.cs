﻿using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proxicall.CRM.Models
{
    public class Opportunity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Owner")]
        [ForeignKey("Owner")]
        public string OwnerId { get; set; }
        public IdentityUser  Owner { get; set; }

        [Required]
        [Display(Name = "Lead")]
        [ForeignKey("Lead")]
        public string LeadId { get; set; }
        public Lead Lead { get; set; }

        [Required]
        [Display(Name = "Product")]
        [ForeignKey("Product")]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        [Display(Name = "Date created")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Estimated closing date")]
        public DateTime? EstimatedCloseDate { get; set; }
        public string Comments { get; set; }
        
        public string Status { get; set; }
        public string Confidence { get; set; }

        public Opportunity()
        {
            CreationDate = DateTime.Now;
            Status = Enumeration.Opportunity.Status.Open.Name;
        }
    }
}