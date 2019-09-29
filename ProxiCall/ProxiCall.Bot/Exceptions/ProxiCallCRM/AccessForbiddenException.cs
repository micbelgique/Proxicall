using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class AccessForbiddenException : Exception
    {
        public AccessForbiddenException(string message) : base(message)
        {
        }

        public AccessForbiddenException() : base()
        {
        }

        public AccessForbiddenException(string message, Exception ex) : base(message, ex)
        {
            
        }
    }
}
