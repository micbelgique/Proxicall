using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpPost]
        public IActionResult ReceiveCall()
        {
            var response = new VoiceResponse();

            // Use <Say> to give the caller some instructions
            response.Say("Hello, Twilio works. Yeay!");

            // Use <Record> to record the caller's message
            //response.Record();

            response.Hangup();

            return TwiML(response);
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            return new ObjectResult("Chuck Norris");
        }
    }
}