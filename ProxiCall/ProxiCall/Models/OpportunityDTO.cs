using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Models
{
    public class OpportunityDTO
    {
        public string OwnerId { get; set; }
        public string LeadId { get; set; }
        public string ProductId { get; set; }
        public DateTime? EstimatedCloseDate { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string Confidence { get; set; }

        public OpportunityDTO()
        {

        }

        public OpportunityDTO(OpportunityDTO opportunity)
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