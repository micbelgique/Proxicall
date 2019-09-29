using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class OpportunityNotCreatedException : Exception
    {
        public OpportunityNotCreatedException() : base()
        {
            
        }

        public OpportunityNotCreatedException(string message) : base(message)
        {
            
        }

        public OpportunityNotCreatedException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
