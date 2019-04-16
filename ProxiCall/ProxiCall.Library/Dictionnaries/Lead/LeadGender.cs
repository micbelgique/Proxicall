using System.Collections.Generic;

namespace ProxiCall.Library.Dictionnaries.Lead
{
    public class LeadGender
    {
        public static readonly string UNDETERMINED = GenderName.Undetermined;
        public static readonly string MALE = GenderName.Male;
        public static readonly string FEMALE = GenderName.Female;

        private static Dictionary<int,string> allGender;

        public static Dictionary<int,string> AllGender
        {
            get
            {
                if(allGender==null)
                {
                    allGender = new Dictionary<int, string>
                    {
                        { 0, GenderName.Undetermined },
                        { 1, GenderName.Male },
                        { 2, GenderName.Female }
                    };
                }
                return allGender;
            }
        }

    }
}
