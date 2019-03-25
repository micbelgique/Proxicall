using System;
using Microsoft.AspNetCore.Mvc;
using ProxiCall.Web.Services.Speech;
using Twilio.AspNet.Core;
using Twilio.Http;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace ProxiCall.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NamesController : TwilioController
    {
        [HttpGet("receive")]
        public IActionResult ReceiveCall()
        {
            var response = new VoiceResponse();
            response.Say("Essai de reconnaissance des noms. Donnez un nom :", voice: "alice", language: Say.LanguageEnum.FrFr);
            var uriAction = new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/names/callback");
            response.Record(action: uriAction, method: HttpMethod.Get, timeout: 2, transcribe: false, playBeep: true);
            return TwiML(response);
        }

        [HttpGet("callback")]
        public IActionResult RecordCallback([FromQuery] string RecordingUrl)
        {
            var resultSTT = CloudSpeechToText.RecognizeSpeechFromUrl(RecordingUrl);
            var response = new VoiceResponse();
            response.Say($"Google a reconnu : {resultSTT}", voice: "alice", language: Say.LanguageEnum.FrFr);
            response.Say("Donnez un autre nom :", voice: "alice", language: Say.LanguageEnum.FrFr);
            var uriAction = new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/names/callback");
            response.Record(action: uriAction, method: HttpMethod.Get, timeout: 2, transcribe: false, playBeep: true);
            return TwiML(response);
        }
    }
}