using System.Collections.Generic;

namespace ProxiCall.Library.Enumeration.Opportunity
{
    public class OpportunityConfidence : Enumeration
    {
        public static OpportunityConfidence High = new HighConfidence();
        public static OpportunityConfidence Average = new AverageConfidence();
        public static OpportunityConfidence Low = new LowConfidence();

        public static IList<string> AllConfidenceDisplay = new List<string>
        {
            High.Name,
            Average.Name,
            Low.Name
        };

        protected OpportunityConfidence(int id, string name)
        : base(id, name)
        { }

        private class HighConfidence : OpportunityConfidence
        {
            public HighConfidence() : base(1, OpportunityConfidenceValue.High)
            { }
        }

        private class AverageConfidence : OpportunityConfidence
        {
            public AverageConfidence() : base(2, OpportunityConfidenceValue.Average)
            { }
        }

        private class LowConfidence : OpportunityConfidence
        {
            public LowConfidence() : base(3, OpportunityConfidenceValue.Low)
            { }
        }
    }
}
