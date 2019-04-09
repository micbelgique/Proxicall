using ProxiCall.Models;
using System.Collections.Generic;

namespace ProxiCall.Dialogs.Shared
{
    public class CRMState
    {
        public Lead Lead { get; set; }
        public Company Company { get; set; }
        public OpportunityDetailed Opportunity { get; set; }
        public Product Product { get; set; }
        public IList<OpportunityDetailed> Opportunities { get; set; }

        public CRMState()
        {
            Lead = new Lead();
            Company = new Company();
            Product = new Product();
            Opportunity = new OpportunityDetailed();
            Opportunities = new List<OpportunityDetailed>();
        }

        public void ResetLead()
        {
            Lead = new Lead();
            if(Opportunities != null)
            {
                Opportunities.Clear();
            }
        }

        public void ResetCompany()
        {
            Company = new Company();
            if (Opportunities != null)
            {
                Opportunities.Clear();
            }
        }

        public void ResetProduct()
        {
            Product = new Product();
        }

        public void ResetOpportunity()
        {
            Lead = new Lead();
            Product = new Product();
            Opportunity = new OpportunityDetailed();
        }
    }
}
