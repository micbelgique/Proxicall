using System;

namespace ProxiCall.Models
{
    public class Opportunity
    {
        public string Id { get; set; }
        public Lead Lead { get; set; }
        public Product Product { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? EstimatedCloseDate { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string Confidence { get; set; }

        public Opportunity()
        {
            Lead = new Lead();
            Product = new Product();
        }

        public void ResetLead()
        {
            Lead = new Lead();
        }

        public void ResetProduct()
        {
            Product = new Product();
        }
    }
}
