using ProxiCall.Library.Enumeration.Opportunity;
using ProxiCall.Library.ProxiCallLuis;
using System;

namespace ProxiCall.Bot.Models
{
    public class Opportunity
    {
        public string OwnerId { get; set; }
        public string LeadId { get; set; }
        public string ProductId { get; set; }
        public DateTime? EstimatedCloseDate { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }

        private string confidence;
        public string Confidence
        {
            get { return confidence; }
            set
            {
                switch(value)
                {
                    //TODO : remove hardcoded part
                    case "incertaine":
                        confidence = OpportunityConfidence.Low.Name;
                        break;
                    case "certaine":
                        confidence = OpportunityConfidence.High.Name;
                        break;
                    case "potentiel":
                    default:
                        confidence = OpportunityConfidence.Average.Name;
                        break;
                }
            }
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