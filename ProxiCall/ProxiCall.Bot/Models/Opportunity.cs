using ProxiCall.Library.Enumeration.Opportunity;
using ProxiCall.Library.ProxiCallLuis;
using System;
using System.Linq;

namespace ProxiCall.Bot.Models
{
    public class Opportunity
    {
        public string OwnerId { get; set; }
        public string LeadId { get; set; }
        public string ProductId { get; set; }
        public DateTime? EstimatedCloseDate { get; set; }
        public string Comments { get; set; }

        public int Status { get; set; }

        public int Confidence { get; set; }

        public void ChangeConfidenceBasedOnName(string confidenceName)
        {
            var dict = OpportunityConfidence.AllConfidenceDisplay;
            var key = dict.FirstOrDefault(x => x.Value.ToLower() == confidenceName.ToLower());
            Confidence = key.Key;
        }
        public void ChangeStatusBasedOnName(string statusName)
        {
            var dict = OpportunityStatus.AllStatusDisplay;
            var key = dict.FirstOrDefault(x => x.Value.ToLower() == statusName.ToLower());
            Status = key.Key;
        }


        public Opportunity()
        {

        }

        public Opportunity(Opportunity opportunity)
        {
            OwnerId = opportunity.OwnerId;
            LeadId = opportunity.LeadId;
            ProductId = opportunity.ProductId;
            EstimatedCloseDate = opportunity.EstimatedCloseDate;
            Comments = opportunity.Comments;
            Status = opportunity.Status;
            Confidence = opportunity.Confidence;
        }
    }
}