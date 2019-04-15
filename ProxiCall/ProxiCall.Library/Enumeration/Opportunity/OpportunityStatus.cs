using System.Collections.Generic;

namespace ProxiCall.Library.Enumeration.Opportunity
{
    public class OpportunityStatus : Enumeration
    {
        public static OpportunityStatus Open = new OpenStatus();
        public static OpportunityStatus Closed = new ClosedStatus();
        public static OpportunityStatus Canceled = new CanceledStatus();

        public static IList<string> AllStatusDisplay = new List<string>
        {
            Open.Name,
            Closed.Name,
            Canceled.Name
        };

        protected OpportunityStatus(int id, string name)
        : base(id, name)
        { }

        private class OpenStatus : OpportunityStatus
        {
            public OpenStatus() : base(1, "Ouverte")
            { }
        }

        private class ClosedStatus : OpportunityStatus
        {
            public ClosedStatus() : base(2, "Fermée")
            { }
        }

        private class CanceledStatus : OpportunityStatus
        {
            public CanceledStatus() : base(3, "Annulée")
            { }
        }
    }
}
