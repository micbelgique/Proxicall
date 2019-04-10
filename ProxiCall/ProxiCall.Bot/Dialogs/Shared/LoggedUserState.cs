using ProxiCall.Bot.Models;

namespace ProxiCall.Bot.Dialogs.Shared
{
    public class LoggedUserState
    {
        public bool WantsToCallButNumberNotFound { get; set; }
        public bool IsEligibleForPotentialSkippingStep { get; set; }
        public bool IsEligibleForPotentialForwarding { get; set; }

        public User LoggedUser { get; set; }

        public LoggedUserState()
        {
            LoggedUser = new User();
        }
    }
}
