using ProxiCall.Models;

namespace ProxiCall.Dialogs.Shared
{
    public class CRMState
    {
        public Lead Lead { get; set; }

        public Company Company { get; set; }
        public string AuthToken { get; set; }

        public CRMState()
        {
            Lead = new Lead();
            Company = new Company();
        }

        public bool WantsToCallButNumberNotFound { get; set; }

        public void ResetLead()
        {
            Lead = new Lead();
        }
    }
}
