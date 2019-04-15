using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class OpportunitiesNotFoundException : Exception
    {
        public OpportunitiesNotFoundException() : base()
        {
            
        }

        public OpportunitiesNotFoundException(string message) : base(message)
        {
            
        }

        public OpportunitiesNotFoundException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
