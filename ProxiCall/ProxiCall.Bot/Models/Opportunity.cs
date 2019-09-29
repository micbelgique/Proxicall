using ProxiCall.Bot.Resources;
using ProxiCall.Library.Enumeration.Opportunity;
using ProxiCall.Library.ProxiCallLuis;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            // TODO : to be improved
            var allConfidenceDisplay = new Dictionary<int, string>
            {
                { 0, OpportunityConfidenceValue.High },
                { 1, OpportunityConfidenceValue.Average },
                { 2, OpportunityConfidenceValue.Low }
            };
            var key = allConfidenceDisplay.FirstOrDefault(x => x.Value.ToLower() == confidenceName.ToLower());
            Confidence = key.Key;
        }
        public void ChangeStatusBasedOnName(string statusName)
        {
            // TODO : to be improved
            var allStatusDisplay = new Dictionary<int, string>
                {
                    { 0, OpportunityStatusValue.Open },
                    { 1, OpportunityStatusValue.Closed },
                    { 2, OpportunityStatusValue.Canceled }
                };
            var key = allStatusDisplay.FirstOrDefault(x => x.Value.ToLower() == statusName.ToLower());
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