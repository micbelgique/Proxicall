using System.Collections.Generic;

namespace Proxicall.CRM.Models.Dictionnaries
{
    public class LevenshteinAllowedDistance
    {
        public const string VERY_SMALL_WORD = "Very Small Word";
        public const string SMALL_WORD = "Small Word";
        public const string MEDIUM_WORD = "Medium Word";
        public const string LONG_WORD = "Long Word";

        private static Dictionary<string,int> allowedDistance;

        public static Dictionary<string, int> AllowedDistance
        {
            get
            {
                if(allowedDistance==null)
                {
                    allowedDistance = new Dictionary<string, int>();
                    allowedDistance.Add(VERY_SMALL_WORD, 0);
                    allowedDistance.Add(SMALL_WORD, 1);
                    allowedDistance.Add(MEDIUM_WORD, 2);
                    allowedDistance.Add(LONG_WORD, 3);
                }
                return allowedDistance;
            }
        }

    }
}
