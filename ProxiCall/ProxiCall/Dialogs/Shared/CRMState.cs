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

        public bool WantsToCallButNoNumberFound { get; set; }
    }
}
