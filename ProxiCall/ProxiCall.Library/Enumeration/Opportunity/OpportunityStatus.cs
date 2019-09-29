using System.Collections.Generic;

namespace ProxiCall.Library.Enumeration.Opportunity
{
    public class OpportunityStatus : Enumeration
    {
        public static OpportunityStatus Open = new OpenStatus();
        public static OpportunityStatus Closed = new ClosedStatus();
        public static OpportunityStatus Canceled = new CanceledStatus();

        public static Dictionary<int, string> AllStatusDisplay = new Dictionary<int, string>
        {
            { Open.Id, Open.Name },
            { Closed.Id, Closed.Name },
            { Canceled.Id, Canceled.Name }
        };

        protected OpportunityStatus(int id, string name)
        : base(id, name)
        { }

        private class OpenStatus : OpportunityStatus
        {
            public OpenStatus() : base(0, OpportunityStatusValue.Open)
            { }
        }

        private class ClosedStatus : OpportunityStatus
        {
            public ClosedStatus() : base(1, OpportunityStatusValue.Closed)
            { }
        }

        private class CanceledStatus : OpportunityStatus
        {
            public CanceledStatus() : base(2, OpportunityStatusValue.Canceled)
            { }
        }
    }
}
