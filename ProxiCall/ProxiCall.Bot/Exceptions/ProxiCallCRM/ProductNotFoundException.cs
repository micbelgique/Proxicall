using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Bot.Exceptions.ProxiCallCRM
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException() : base()
        {
            
        }

        public ProductNotFoundException(string message) : base(message)
        {
            
        }

        public ProductNotFoundException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
