using System.Collections.Generic;

namespace ProxiCall.Bot.Dialogs.Shared
{
    public class LuisState
    {
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
