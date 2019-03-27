using ProxiCall.Models;
using System.Text;

namespace ProxiCall.Dialogs.Shared
{
    public class LeadState
    {
        public Lead Lead { get; set; }

        public LeadState()
        {
            Lead = new Lead();
        }
    }
}
