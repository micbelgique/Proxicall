using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector.DirectLine;
using ProxiCall.Web.Models;
using ProxiCall.Web.Services;

namespace ProxiCall.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var botConnector = new BotConnector();
            await botConnector.StartWebsocket(OnMessageReceivedHandler);
            var activitiesFromBot = ;
            return View();
        }

        private Activity OnMessageReceivedHandler(IList<Activity> botReply)
        {
            throw new NotImplementedException();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
