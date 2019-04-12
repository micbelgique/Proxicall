using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base()
        {
            
        }

        public InvalidTokenException(string message) : base(message)
        {
                
        }

        public InvalidTokenException(string message, Exception ex) : base(message, ex)
        {
                
        }
    }
}
