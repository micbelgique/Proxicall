using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Connector.DirectLine;
using ProxiCall.Web.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : TwilioController
    {
        //private readonly string Sid = Environment.GetEnvironmentVariable("TwilioSid");
        //private readonly string Token = Environment.GetEnvironmentVariable("TwilioToken");
        private IList<string> _messages;

        private readonly IActionContextAccessor _actionContextAccessor;

        public VoiceController(IActionContextAccessor actionContextAccessor)
        {
            _actionContextAccessor = actionContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveCallAsync()
        {
            var botConnector = new BotConnector();

            await botConnector.ReceiveMessagesFromBotAsync(ReceiveMessageFromBot);
            // Use <Say> to give the caller some instructions
            //foreach(var message in _messages)
            //{
            //    megastring += message;
            //}
            var response = new VoiceResponse();
            response.Say("Bye");

            // Use <Record> to record the caller's message
            //response.Record();

            response.Hangup();

            return TwiML(response);
        }

        private void ReceiveMessageFromBot(IList<Activity> botReplies)
        {
            var response = new VoiceResponse();

            response.Say(botReplies[0].Text);

            TwiML(response).ExecuteResultAsync(_actionContextAccessor.ActionContext).RunSynchronously();
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return new ObjectResult("Chuck Norris");
        }
    }
}