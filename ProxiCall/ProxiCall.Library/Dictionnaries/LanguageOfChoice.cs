using System;
using System.Collections.Generic;
using System.Text;

namespace ProxiCall.Library.Dictionnaries
{
    public class LanguageOfChoice
    {
        public const string EN_US = "en-US";
        public const string FR_FR = "fr-FR";
        public const string DEFAULT = EN_US;

        private Dictionary<string, string> allowedLanguageOfChoice;

        public Dictionary<string,string> AllowedLanguageOfChoice
        {
            get
            {
                if (allowedLanguageOfChoice == null)
                {
                    allowedLanguageOfChoice = new Dictionary<string, string>
                    {
                        { EN_US, "English (US)" },
                        { FR_FR, "Français" }
                    };
                }
                return allowedLanguageOfChoice;
            }
        }
    }
}
