using ProxiCall.Models;
using System.Collections.Generic;

namespace ProxiCall.Dialogs.Shared
{
    public class CRMState
    {
        public Lead Lead { get; set; }
        public Company Company { get; set; }
        public Opportunity Opportunity { get; set; }
        public IList<Opportunity> Opportunities { get; set; }

        public CRMState()
        {
            Lead = new Lead();
            Company = new Company();
            Opportunity = new Opportunity();
            Opportunities = new List<Opportunity>();
        }

        public bool WantsToCallButNumberNotFound { get; set; }
        public bool IsEligibleForPotentialForwarding { get; set; }

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

        public void ResetOpportunity()
        {
            Opportunity = new Opportunity();
        }
    }
}
