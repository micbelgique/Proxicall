using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Connector.DirectLine;
using ProxiCall.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Web.Models
{
    public class IndexModel : PageModel
    {
        private readonly BotConnector _botConnector;

        [BindProperty]
        public IList<Activity> Activities { get; set; }

        [BindProperty]
        public string UserReply { get; set; }

        public IndexModel()
        {
            _botConnector = new BotConnector();
            _botConnector.StartWebsocket(OnMessageReceivedHandler);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var activity = new Activity();
            activity.From = new ChannelAccount("webuserid", "webuser");
            activity.Text = UserReply;

            await _botConnector.SendMessageAsync(activity);

            return RedirectToPage("/Index");
        }

        public async Task SendMessage()
        {
            var activity = new Activity();
            activity.From = new ChannelAccount("webuserid", "webuser");
            activity.Text = UserReply;

            await _botConnector.SendMessageAsync(activity);
        }

        private void OnMessageReceivedHandler(IList<Activity> botReplies)
        {
            Activities = botReplies;
        }
    }
}
