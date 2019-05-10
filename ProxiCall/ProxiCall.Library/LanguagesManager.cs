using System.Collections.Generic;

namespace ProxiCall.Library
{
    public class LanguagesManager
    {
        public const string EN_US = "en-US";
        public const string FR_FR = "fr-FR";
        public const string DEFAULT = EN_US;

        private Dictionary<string, string> allowedLanguagesOfChoice;

        public Dictionary<string,string> AllowedLanguagesOfChoice
        {
            get
            {
                if (allowedLanguagesOfChoice == null)
                {
                    allowedLanguagesOfChoice = new Dictionary<string, string>
                    {
                        { EN_US, "English (US)" },
                        { FR_FR, "Français" }
                    };
                }
                return allowedLanguagesOfChoice;
            }
        }

        public string CheckAndReturnAppropriateCulture(string localeName)
        {
            var isAnAcceptedLanguage = AllowedLanguagesOfChoice.TryGetValue(localeName, out var localeFullName);

            if (isAnAcceptedLanguage)
            {
                return localeName;
            }
            else
            {
                return DEFAULT;
            }
        }
    }
}
