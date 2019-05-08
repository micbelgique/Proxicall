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
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;

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
        private readonly string[] _names = 
        {
            // Leads
            "Mélissa Fontesse","Arthur Grailet","Stéphanie Bémelmans","Renaud Dumont","Vivien Preser","Massimo Gentile","Thomas D'Hollander","Simon Gauthier","Laura Lieu","Tinaël Devresse",
            "Andy Dautricourt","Julien Dendauw","Martine Meunier","Nathan Pire","Maxime Hempte","Victor Pastorani","Tobias Jetzen","Xavier Tordoir","Loris Rossi","Jessy Delhaye","Sylvain Duhant",
            "David Vanni","Simon Fauconnier","Chloé Michaux","Xavier Vercruysse","Xavier Bastin","Guillaume Rigaux","Romain Blondeau","Laïla Valenti","Ryan Büttner","Pierre Mayeur","Guillaume Servais",
            "Frédéric Carbonnelle","Valentin Chevalier","Alain Musoni",
            // Companies
            "Smart Richesse","Microsoft Innovation Center","Doctor Love","Proximus EnCo","Seeing AI",
            // Products
            "BotBot",
            "SpeakAnotherDayBot",
            "OnceUponADreamBot",
            "CheerlyBot",
            "TormentorBot",
            "SpiderBot"
        };

        private readonly string _hints;

        public VoiceController(IActionContextAccessor actionContextAccessor, IHostingEnvironment hostingEnvironment)
        {
            _actionContextAccessor = actionContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            TwilioClient.Init(Sid, Token);
            var sb = new StringBuilder();
            foreach (var name in _names)
            {
                sb.Append(name);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            _hints = sb.ToString();
        }
        
        //-------------------
        //---OUTBOUND CALL---
        //-------------------
        [HttpGet("outbound/{to}")]
        public IActionResult OutboundCall(string to)
        {
            CallResource.Create(
                method: HttpMethod.Get,
                url: new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/voice/receive"),
                to: new Twilio.Types.PhoneNumber(to),
                from: new Twilio.Types.PhoneNumber(Environment.GetEnvironmentVariable("TwilioPhoneNumber"))
            );
            return Ok();
        }

        //------------
        //---SHARED---
        //------------
        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveCall([FromQuery] string CallSid, [FromQuery] string From, [FromQuery] string To)
        {
            var audioToDelete = Directory.GetFiles(_hostingEnvironment.WebRootPath + "/audio");
            foreach (var file in audioToDelete)
            {
                System.IO.File.Delete(file);
            }
            var xmlToDelete = Directory.GetFiles(_hostingEnvironment.WebRootPath + "/xml");
            foreach (var file in xmlToDelete)
            {
                System.IO.File.Delete(file);
            }

            _botConnector = new BotConnector(CallSid);

            _ = System.Threading.Tasks.Task.Run(() => _botConnector.ReceiveMessagesFromBotAsync(HandleIncomingBotMessagesAsync));

            var activity = new Activity
            {
                From = new ChannelAccount("TwilioUserId", "TwilioUser"),
                Type = ActivityTypes.Message,
                Text = string.Empty,
                Entities = new List<Entity>()
            };

            var phoneNumber = string.Empty;
            if (From != Environment.GetEnvironmentVariable("TwilioPhoneNumber"))
            {
                //User Phone Number during Inbound Call
                phoneNumber = From.Substring(1);
            }
            else
            {
                //User Phone Number during Outbound Call
                phoneNumber = To.Substring(1);
            }

            var entity = new Entity
            {
                Properties = new JObject
                {
                    {
                        "firstmessage", JToken.Parse(phoneNumber)
                    }
                }
            };
            activity.Entities.Add(entity);

            await _botConnector.SendMessageToBotAsync(activity);

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
            var error = false;
            var errorMessage = string.Empty;

            foreach (var activity in botReplies)
            {
                CultureInfo.CurrentCulture = new CultureInfo(activity.Locale);

                var test = CultureInfo.CurrentCulture;
                var test2 = activity.Locale;

                var tts = new TextToSpeech();
                //Using TTS to repond to the caller
                var ttsResponse = await System.Threading.Tasks.Task.Run(() =>
                tts.TransformTextToSpeechAsync(activity.Text, CultureInfo.CurrentCulture.Name));

                var wavGuid = Guid.NewGuid();
                var pathToAudioDirectory = _hostingEnvironment.WebRootPath + "/audio";
                var pathCombined = Path.Combine(pathToAudioDirectory, $"{ wavGuid }.wav");

                await FormatConvertor.TurnAudioStreamToFile(ttsResponse, pathCombined);

                voiceResponse.Play(new Uri($"{Environment.GetEnvironmentVariable("Host")}/audio/{wavGuid}.wav"));

                if (activity.Entities != null)
                {
                    foreach (var entity in activity.Entities)
                    {
                        forward = entity.Properties.TryGetValue("forward", out var numberJToken);
                        forwardingNumber = forward ? numberJToken.ToString() : string.Empty;

                        error = entity.Properties.TryGetValue("error", out var errorMessageJToken);
                        if (error)
                        {
                            break;
                        }
                    }
                }
            }

            if (error)
            {
                voiceResponse.Hangup();
            }
            else if(forward)
            {
                voiceResponse.Dial(number: forwardingNumber);
            }
            else
            {
                voiceResponse.Gather(
                    input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
                    language: CultureInfo.CurrentCulture.Name,
                    action: new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/voice/send"),
                    method: Twilio.Http.HttpMethod.Get,
                    speechTimeout: "auto",
                    hints: _hints
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
            var activityToSend = new Activity
            {
                From = new ChannelAccount("TwilioUserId", "TwilioUser"),
                Type = ActivityTypes.Message,
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
    }
}