using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proxicall.CRM.Models.Enumeration.Opportunity
{
    public class Status : Enumeration
    {
        public static Status Open = new OpenStatus();
        public static Status Closed = new ClosedStatus();
        public static Status Canceled = new CanceledStatus();

        protected Status(int id, string name)
        : base(id, name)
        { }

        private class OpenStatus : Status
        {
            public OpenStatus() : base(1, "Ouverte")
            { }
        }

        private class ClosedStatus : Status
        {
            public ClosedStatus() : base(2, "Fermée")
            { }
        }

        private class CanceledStatus : Status
        {
            public CanceledStatus() : base(3, "Annulée")
            { }
        }
    }
}
