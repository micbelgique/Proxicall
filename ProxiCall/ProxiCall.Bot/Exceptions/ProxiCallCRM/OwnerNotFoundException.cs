using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class OwnerNotFoundException : Exception
    {
        public OwnerNotFoundException() : base()
        {
            
        }

        public OwnerNotFoundException(string message) : base(message)
        {
            
        }

        public OwnerNotFoundException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
