using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class CompanyNotFoundException : Exception
    {
        public CompanyNotFoundException() : base()
        {
            
        }

        public CompanyNotFoundException(string message) : base(message)
        {
            
        }

        public CompanyNotFoundException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
