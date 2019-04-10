using System.Collections.Generic;

namespace ProxiCall.CRM.Models.Enumeration.Opportunity
{
    public class Confidence : Enumeration
    {
        public static Confidence High = new HighConfidence();
        public static Confidence Average = new AverageConfidence();
        public static Confidence Low = new LowConfidence();

        public static IList<string> AllConfidenceDisplay = new List<string>
        {
            High.Name,
            Average.Name,
            Low.Name
        };

        protected Confidence(int id, string name)
        : base(id, name)
        { }

        private class HighConfidence : Confidence
        {
            public HighConfidence() : base(1, "Haute")
            { }
        }

        private class AverageConfidence : Confidence
        {
            public AverageConfidence() : base(2, "Moyenne")
            { }
        }

        private class LowConfidence : Confidence
        {
            public LowConfidence() : base(3, "Basse")
            { }
        }
    }
}
