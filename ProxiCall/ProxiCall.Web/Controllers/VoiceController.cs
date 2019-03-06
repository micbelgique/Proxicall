using Microsoft.AspNetCore.Mvc;
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
        private IList<String> _messages;
        [HttpPost]
        public IActionResult ReceiveCall()
        {
            _messages = new List<String>();
            var botConnector = new BotConnector();
            var response = new VoiceResponse();

            botConnector.ReceiveMessagesFromBotAsync(ReceiveMessageFromBot).Wait();
            // Use <Say> to give the caller some instructions
            foreach(var message in _messages)
            {
                response.Say(message);
            }

            // Use <Record> to record the caller's message
            //response.Record();

            response.Hangup();

            return TwiML(response);
        }

        private void ReceiveMessageFromBot(IList<Activity> botReplies)
        {
            _messages.Clear();
            foreach(var activity in botReplies)
            {
                _messages.Add(activity.Text);
            }
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return new ObjectResult("Chuck Norris");
        }
    }
}