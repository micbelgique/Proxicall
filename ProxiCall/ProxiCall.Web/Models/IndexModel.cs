using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Web.Models
{
    public class IndexModel : PageModel
    {
        //[BindProperty]
        public IList<Activity> activities;
    }
}
