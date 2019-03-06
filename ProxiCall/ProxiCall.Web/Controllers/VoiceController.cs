using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Connector.DirectLine;
using ProxiCall.Web.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : TwilioController
    {
        //private readonly string Sid = Environment.GetEnvironmentVariable("TwilioSid");
        //private readonly string Token = Environment.GetEnvironmentVariable("TwilioToken");
        private static BotConnector _botConnector;
        private readonly IActionContextAccessor _actionContextAccessor;

        public VoiceController(IActionContextAccessor actionContextAccessor)
        {
            _actionContextAccessor = actionContextAccessor;
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveCallAsync()
        {
            _botConnector = new BotConnector();

            await _botConnector.ReceiveMessagesFromBotAsync(ReceiveMessageFromBot);
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

            foreach(var activity in botReplies)
            {
                response.Say(activity.Text, voice: "alice", language: "fr-FR");
            }
            response.Gather(
                input: new List<Gather.InputEnum> { Gather.InputEnum.Speech }, 
                language: Gather.LanguageEnum.FrFr, 
                action: new Uri("https://3178f91b.ngrok.io/api/voice/reply"), 
                method: Twilio.Http.HttpMethod.Get, 
                speechTimeout: "auto"
            );

            //TODO .Start or .RunSynchronously ?
            TwiML(response).ExecuteResultAsync(_actionContextAccessor.ActionContext).RunSynchronously();
        }

        [HttpGet("reply")]
        public async Task<IActionResult> UserReply([FromQuery] string SpeechResult, [FromQuery] double Confidence)
        {
            var activity = new Activity();
            activity.From = new ChannelAccount("TwilioUserId", "TwilioUser");
            activity.Type = "message";
            activity.Text = SpeechResult;

            await _botConnector.SendMessageToBotAsync(activity);

            var response = new VoiceResponse();

            response.Pause(30);
            response.Say("Le bot ne répond pas", voice: "alice", language: "fr-FR");

            return TwiML(response);
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return new ObjectResult("Chuck Norris");
        }
    }
}