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
        public static readonly string SEARCH_EMAIL_ENTITYNAME = "searchEmail";
        public static readonly string SEARCH_CONTACT_ENTITYNAME = "searchContact";
        public static readonly string SEARCH_CONTACT_NAME_ENTITYNAME = "searchContactName";

        public string IntentName { get; set; }

        private IList<string> entities;

        public IList<string> Entities
        {
            get
            {
                if (entities == null)
                    entities = new List<string>();
                return entities;
            }
            set
            {
                entities = value;
            }
        }

        public bool AddDetectedEntity(string detectedEntity)
        {
            if(!Entities.Contains(detectedEntity))
            {
                Entities.Add(detectedEntity);
                return true;
            }
            return false;
        }

        public bool RemoveDetectedEntity(string removedDetectedEntity)
        {
            return Entities.Remove(removedDetectedEntity);
        }

        public void ResetIntentIfNoEntities()
        {
            if(Entities.Count == 0)
            {
                IntentName = string.Empty;
            }
        }

        public void ResetAll()
        {
            IntentName = string.Empty;
            Entities.Clear();
        }
    }
}
