using ProxiCall.Models;

namespace ProxiCall.Dialogs.Shared
{
    public class CRMState
    {
        public Lead Lead { get; set; }

        public CRMState()
        {
            Lead = new Lead();
        }

        public bool WantsToCallButNumberNotFound { get; set; }

        public bool FullNameIncomplete { get; set; }

        public void ResetLead()
        {
            Lead = new Lead();
        }
    }
}
