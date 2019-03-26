using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly string[] _names = new string[] {"Mélissa Fontesse","Arthur Grailet","Stéphanie Bémelmans","Renaud Dumont","Vivien Preser","Massimo Gentile","Thomas D'Hollander","Simon Gauthier","Laura Lieu","Tinaël Devresse",
                                                        "Andy Dautricourt","Julien Dendauw","Martine Meunier","Nathan Pire","Maxime Hempte","Victor Pastorani","Tobias Jetzen","Xavier Tordoir","Loris Rossi","Jessy Delhaye","Sylvain Duhant",
                                                        "David Vanni","Simon Fauconnier","Chloé Michaux","Xavier Vercruysse","Xavier Bastin","Guillaume Rigaux","Romain Blondeau","Laïla Valenti","Ryan Büttner","Pierre Mayeur","Guillaume Servais",
                                                        "Frédéric Carbonnelle","Valentin Chevalier","Alain Musoni"};

        private readonly string _hints;

        public NamesController()
        {
            var sb = new StringBuilder();
            foreach(var name in _names)
            {
                sb.Append(name);
                sb.Append(",");
            }
            sb.Remove(sb.Length-1, 1);
            _hints = sb.ToString();
        }

    [HttpGet("receive")]
        public IActionResult ReceiveCall()
        {
            var response = new VoiceResponse();
            response.Say("Essai de reconnaissance des noms. Donnez un nom :", voice: "alice", language: Say.LanguageEnum.FrFr);
            var uriAction = new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/names/callback");
            response.Gather(
                    input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
                    language: Gather.LanguageEnum.FrFr,
                    action: new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/names/callback"),
                    method: HttpMethod.Get,
                    speechTimeout: "auto",
                    hints: _hints
                );
            return TwiML(response);
        }

        [HttpGet("callback")]
        public IActionResult RecordCallback([FromQuery] string SpeechResult)
        {
            //var resultSTT = CloudSpeechToText.RecognizeSpeechFromUrl(RecordingUrl);
            Console.WriteLine($"\n------------------------------------------------------\n\t\t\t\t\t\t\t\t\t{SpeechResult}\n------------------------------------------------------\n");
            var response = new VoiceResponse();
            response.Say("Donnez un autre nom :", voice: "alice", language: Say.LanguageEnum.FrFr);
            var uriAction = new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/names/callback");
            response.Gather(
                    input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
                    language: Gather.LanguageEnum.FrFr,
                    action: new Uri($"{Environment.GetEnvironmentVariable("Host")}/api/names/callback"),
                    method: HttpMethod.Get,
                    speechTimeout: "auto",
                    hints: _hints
                );
            return TwiML(response);
        }
    }
}