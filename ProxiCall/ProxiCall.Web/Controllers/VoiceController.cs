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
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;

namespace ProxiCall.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : TwilioController
    {
        private readonly string Sid = Environment.GetEnvironmentVariable("TwilioSid");
        private readonly string Token = Environment.GetEnvironmentVariable("TwilioToken");
        private static BotConnector _botConnector;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;

        public VoiceController(IActionContextAccessor actionContextAccessor, IHostingEnvironment hostingEnvironment)
        {
            _actionContextAccessor = actionContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            TwilioClient.Init(Sid, Token);
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveCall([FromQuery] string CallSid)
        {
            _botConnector = new BotConnector(CallSid);

            _botConnector.ReceiveMessagesFromBotAsync(ReceiveMessageFromBot);

            var response = new VoiceResponse();
            response.Say("", voice: "alice", language: "fr-FR");
            response.Pause(15);

            return TwiML(response);
        }

        private void ReceiveMessageFromBot(IList<Activity> botReplies, string callSid)
        {
            var response = new VoiceResponse();
            var says = new StringBuilder();
            foreach (var activity in botReplies)
            {
                response.Say(activity.Text, voice: "alice", language: "fr-FR");
            }
            response.Gather(
                input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
                language: Gather.LanguageEnum.FrFr,
                action: new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/voice/send"),
                method: Twilio.Http.HttpMethod.Get,
                speechTimeout: "auto"
            );

            var fileName = Guid.NewGuid();
            var path = _hostingEnvironment.WebRootPath + "/xml";
            System.IO.File.WriteAllText($"{path}/{fileName}.xml", response.ToString());

            var call = CallResource.Update(
                method: Twilio.Http.HttpMethod.Get,
                url: new Uri($"{Environment.GetEnvironmentVariable("Host")}/xml/{fileName}.xml"),
                pathSid: callSid
            );
        }

        [HttpGet("send")]
        public IActionResult UserReply([FromQuery] string SpeechResult, [FromQuery] double Confidence, [FromQuery] string CallSid)
        {
            //var files = Directory.GetFiles(_hostingEnvironment.WebRootPath + "/xml");
            //foreach(var file in files)
            //{
            //    System.IO.File.Delete(file);
            //}

            var activity = new Activity();
            activity.From = new ChannelAccount("TwilioUserId", "TwilioUser");
            activity.Type = "message";
            activity.Text = SpeechResult;

            _botConnector.SendMessageToBotAsync(activity);

            var response = new VoiceResponse();
            response.Pause(15);
            response.Say("Le botte ne répond pas.", voice: "alice", language: "fr-FR"); //Bot is mispelled for phonetic purpose

            return TwiML(response);
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return new ObjectResult("Chuck Norris");
        }
    }
}