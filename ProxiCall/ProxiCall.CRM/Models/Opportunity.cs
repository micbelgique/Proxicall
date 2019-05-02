using ProxiCall.Library.Enumeration.Opportunity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxiCall.CRM.Models
{
    public class Opportunity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Owner")]
        [ForeignKey("Owner")]
        public string OwnerId { get; set; }
        public ApplicationUser  Owner { get; set; }

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
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Estimated closing date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EstimatedCloseDate { get; set; }

        public string Comments { get; set; }
        
        public int Status { get; set; }

        [NotMapped]
        public string StatusName
        {
            get
            {
                // TODO : to be improved
                var allStatusDisplay = new Dictionary<int, string>
                {
                    { 0, OpportunityStatusValue.Open },
                    { 1, OpportunityStatusValue.Closed },
                    { 2, OpportunityStatusValue.Canceled }
                };
                allStatusDisplay.TryGetValue(Status, out string statusName);
                return statusName;
            }
        }

        public int Confidence { get; set; }
        
        [NotMapped]
        public string ConfidenceName
        {
            get
            {
                // TODO : to be improved
                var allConfidenceDisplay = new Dictionary<int, string>
                {
                    { 0, OpportunityConfidenceValue.High },
                    { 1, OpportunityConfidenceValue.Average },
                    { 2, OpportunityConfidenceValue.Low }
                };
                allConfidenceDisplay.TryGetValue(Confidence, out string confidenceName);
                return confidenceName;
            }
        }


        public Opportunity()
        {
            CreationDate = DateTime.Now.Date;
            Status = OpportunityStatus.Open.Id;
        }
    }
}
