using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class LeadNotFoundException : Exception
    {
        public LeadNotFoundException() : base()
        {
            
        }

        public LeadNotFoundException(string message) : base(message)
        {
            
        }

        public LeadNotFoundException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
