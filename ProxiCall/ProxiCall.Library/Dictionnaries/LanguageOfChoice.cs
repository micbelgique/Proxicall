using System;
using System.Collections.Generic;
using System.Text;

namespace ProxiCall.Library.Dictionnaries
{
    public class LanguageOfChoice
    {
        public const string EN = "en";
        public const string EN_US = "en-us";
        public const string EN_UK = "en-uk";
        public const string FR = "fr";
        public const string FR_FR = "fr-fr";

        private Dictionary<string, string> allowedLanguageOfChoice;

        public Dictionary<string,string> AllowedLanguageOfChoice
        {
            get
            {
                if (allowedLanguageOfChoice == null)
                {
                    allowedLanguageOfChoice = new Dictionary<string, string>
                    {
                        { EN, "English" },
                        { EN_US, "English" },
                        { EN_UK, "English" },
                        { FR, "Français" },
                        { FR_FR, "Français" }
                    };
                }
                return allowedLanguageOfChoice;
            }
        }


        private Dictionary<string, string> displayedLanguageOfChoice;

        public Dictionary<string, string> DisplayedLanguageOfChoice
        {
            get
            {
                if (displayedLanguageOfChoice == null)
                {
                    displayedLanguageOfChoice = new Dictionary<string, string>
                    {
                        { EN, "English" },
                        { FR, "Français" }
                    };
                }
                return displayedLanguageOfChoice;
            }
        }
    }
}
