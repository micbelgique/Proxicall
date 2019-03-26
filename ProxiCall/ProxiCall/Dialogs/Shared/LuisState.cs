using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Dialogs.Shared
{
    public class LuisState
    {
        public static readonly string SEARCH_PHONENUMBER_ENTITYNAME = "searchPhoneNumber";
        public static readonly string SEARCH_ADDRESS_ENTITYNAME = "searchAddress";
        public static readonly string SEARCH_COMPANY_ENTITYNAME = "searchCompany";

        public string IntentName { get; set; }

        public bool userWantsAllInformations { get; set; }

        private IList<string> detectedEntities;

        public IList<string> DetectedEntities
        {
            get
            {
                if (detectedEntities == null)
                    detectedEntities = new List<string>();
                return detectedEntities;
            }
            set { detectedEntities = value; }
        }

        public bool AddDetectedEntity(string detectedEntity)
        {
            if(!DetectedEntities.Contains(detectedEntity))
            {
                DetectedEntities.Add(detectedEntity);
                return true;
            }
            return false;
        }

        public bool RemoveDetectedEntity(string removedDetectedEntity)
        {
            return DetectedEntities.Remove(removedDetectedEntity);
        }

        public void ResetIntentIfNoEntities()
        {
            if(DetectedEntities.Count == 0)
            {
                IntentName = string.Empty;
            }
        }

        public void ResetAll()
        {
            IntentName = string.Empty;
            DetectedEntities.Clear();
            userWantsAllInformations = false;
        }
    }
}
