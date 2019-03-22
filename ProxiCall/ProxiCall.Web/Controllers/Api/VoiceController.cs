using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.AspNetCore.Hosting;
using ProxiCall.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Twilio.AspNet.Core;
using System.Threading.Tasks;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using ProxiCall.Web.Services.Speech;
using Twilio.Http;

namespace ProxiCall.Web.Controllers.Api
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
        public IActionResult ReceiveCall([FromQuery] string CallSid)
        {
            _botConnector = new BotConnector(CallSid);
            
            System.Threading.Tasks.Task.Run(() => _botConnector.ReceiveMessagesFromBotAsync(HandleIncomingBotMessagesAsync));
            
            //Preventing the call from hanging up (/receive needs to return a TwiML)
            var response = new VoiceResponse();
            response.Say("", voice: "alice", language: Say.LanguageEnum.FrFr);
            response.Pause(15);

            return TwiML(response);
        }

        private async System.Threading.Tasks.Task HandleIncomingBotMessagesAsync(IList<Activity> botReplies, string callSid)
        {
            var voiceResponse = new VoiceResponse();
            var says = new StringBuilder();
            var forwardingNumber = string.Empty;
            var forward = false;

            var filesToDelete = Directory.GetFiles(_hostingEnvironment.WebRootPath + "/audio");
            foreach (var file in filesToDelete)
            {
                System.IO.File.Delete(file);
            }

            foreach (var activity in botReplies)
            {
                //Using TTS to repond to the caller
                var ttsResponse = await System.Threading.Tasks.Task.Run(() =>
                TextToSpeech.TransformTextToSpeechAsync(activity.Text, "fr-FR"));

                var wavGuid = Guid.NewGuid();
                var pathToAudioDirectory = _hostingEnvironment.WebRootPath + "/audio";
                var pathCombined = Path.Combine(pathToAudioDirectory, $"{ wavGuid }.wav");

                await FormatConvertor.TurnAudioStreamToFile(ttsResponse, pathCombined);

                voiceResponse.Play(new Uri($"{Environment.GetEnvironmentVariable("Host")}/audio/{wavGuid}.wav"));

                if (activity.Entities != null)
                {
                    foreach (var entity in activity.Entities)
                    {
                        forward = entity.Properties.TryGetValue("forward", out var jtoken);
                        forwardingNumber = forward ? jtoken.ToString() : string.Empty;
                    }
                }
            }
            
            if(forward)
            {
                voiceResponse.Dial(number: forwardingNumber);
            }
            else
            {
                voiceResponse.Gather(
                    input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
                    language: Gather.LanguageEnum.FrFr,
                    action: new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/voice/send"),
                    method: Twilio.Http.HttpMethod.Get,
                    speechTimeout: "auto"
                );
            }

            var xmlFileName = Guid.NewGuid();
            var pathToXMLDirectory = _hostingEnvironment.WebRootPath + "/xml";
            System.IO.File.WriteAllText($"{pathToXMLDirectory}/{xmlFileName}.xml", voiceResponse.ToString());

            CallResource.Update(
                method: Twilio.Http.HttpMethod.Get,
                url: new Uri($"{Environment.GetEnvironmentVariable("Host")}/xml/{xmlFileName}.xml"),
                pathSid: callSid
            );
        }

        [HttpGet("send")]
        public async Task<IActionResult> SendUserMessageToBot([FromQuery] string SpeechResult, [FromQuery] double Confidence, [FromQuery] string CallSid)
        {
            var filesToDelete = Directory.GetFiles(_hostingEnvironment.WebRootPath + "/xml");
            foreach (var file in filesToDelete)
            {
                System.IO.File.Delete(file);
            }

            var activityToSend = new Activity
            {
                From = new ChannelAccount("TwilioUserId", "TwilioUser"),
                Type = "message",
                Text = SpeechResult
            };
            
            await _botConnector.SendMessageToBotAsync(activityToSend);

            //Preventing the call from hanging up
            var response = new VoiceResponse();
            response.Pause(15);

            //DEBUG
            response.Say("Aucune réponse de ProxiCall", voice: "alice", language: Say.LanguageEnum.FrFr);

            return TwiML(response);
        }

        [HttpGet("record")]
        public async Task<IActionResult> RecordVoiceOfUserAsync([FromQuery] string RecordingUrl)
        {
            var resultSTT = await SpeechToText.RecognizeSpeechFromUrlAsync(RecordingUrl, "fr-FR");

            var filesToDelete = Directory.GetFiles(_hostingEnvironment.WebRootPath + "/xml");
            foreach (var file in filesToDelete)
            {
                System.IO.File.Delete(file);
            }

            var activityToSend = new Activity
            {
                From = new ChannelAccount("TwilioUserId", "TwilioUser"),
                Type = "message",
                Text = resultSTT
            };

            //TODO : async not as async?
            await _botConnector.SendMessageToBotAsync(activityToSend);

            //Preventing the call from hanging up
            var response = new VoiceResponse();
            response.Pause(15);

            //DEBUG
            response.Say("Aucune réponse de ProxiCall", voice: "alice", language: Say.LanguageEnum.FrFr);

            return TwiML(response);
        }
    }
}