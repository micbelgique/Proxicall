using System.Collections.Generic;

namespace ProxiCall.Library.Enumeration.Opportunity
{
    public class OpportunityConfidence : Enumeration
    {
        public static OpportunityConfidence High = new HighConfidence();
        public static OpportunityConfidence Average = new AverageConfidence();
        public static OpportunityConfidence Low = new LowConfidence();

        public static Dictionary<int,string> AllConfidenceDisplay = new Dictionary<int,string>
        {
            { High.Id, High.Name },
            { Average.Id, Average.Name },
            { Low.Id, Low.Name }
        };

        protected OpportunityConfidence(int id, string name)
        : base(id, name)
        { }

        private class HighConfidence : OpportunityConfidence
        {
            public HighConfidence() : base(0, OpportunityConfidenceValue.High)
            { }
        }

        private class AverageConfidence : OpportunityConfidence
        {
            public AverageConfidence() : base(1, OpportunityConfidenceValue.Average)
            { }
        }

        private class LowConfidence : OpportunityConfidence
        {
            public LowConfidence() : base(2, OpportunityConfidenceValue.Low)
            { }
        }
    }
}
