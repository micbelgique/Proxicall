using System.Collections.Generic;

namespace ProxiCall.Library.Dictionnaries
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
                    allowedDistance = new Dictionary<string, int>
                    {
                        { VERY_SMALL_WORD, 0 },
                        { SMALL_WORD, 1 },
                        { MEDIUM_WORD, 2 },
                        { LONG_WORD, 3 }
                    };
                }
                return allowedDistance;
            }
        }

    }
}
