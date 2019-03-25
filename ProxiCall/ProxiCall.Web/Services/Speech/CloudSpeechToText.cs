using Google.Cloud.Speech.V1;
using Google.Protobuf.Collections;
using System.Text;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace ProxiCall.Web.Services.Speech
{
    public class CloudSpeechToText
    {
        public static string RecognizeSpeechFromUrl(string url)
        {
            RecognitionAudio audio = RecognitionAudio.FetchFromUri(url);
            SpeechClient client = SpeechClient.Create();

            SpeechContext fullnames = new SpeechContext();
            var hints = new string[] {"Mélissa Fontesse",
                                        "Arthur Grailet",
                                        "Stéphanie Bémelmans",
                                        "Renaud Dumont",
                                        "Vivien Preser",
                                        "Massimo Gentile",
                                        "Thomas D'Hollander",
                                        "Simon Gauthier",
                                        "Laura Lieu",
                                        "Tinaël Devresse",
                                        "Andy Dautricourt",
                                        "Julien Dendauw",
                                        "Martine Meunier",
                                        "Nathan Pire",
                                        "Maxime Hempte",
                                        "Victor Pastorani",
                                        "Tobias Jetzen",
                                        "Xavier Tordoir",
                                        "Loris Rossi",
                                        "Jessy Delhaye",
                                        "Sylvain Duhant",
                                        "David Vanni",
                                        "Simon Fauconnier",
                                        "Chloé Michaux",
                                        "Xavier Vercruysse",
                                        "Xavier Bastin",
                                        "Guillaume Rigaux",
                                        "Romain Blondeau",
                                        "Laïla Valenti",
                                        "Ryan Büttner",
                                        "Pierre Mayeur",
                                        "Guillaume Servais",
                                        "Frédéric Carbonnelle",
                                        "Valentin Chevalier",
                                        "Alain Musoni"
            };

            foreach(var name in hints)
            {
                fullnames.Phrases.Add(name);
            }

            RecognitionConfig config = new RecognitionConfig
            {
                Encoding = AudioEncoding.Linear16,
                SampleRateHertz = 8000,
                LanguageCode = LanguageCodes.French.France,
                SpeechContexts = { fullnames }
            };

            RecognizeResponse response = client.Recognize(config, audio);
            var sttResult = new StringBuilder(string.Empty);
            foreach (var result in response.Results)
            {
                sttResult.Append(result.Alternatives[0].Transcript);
            }

            return sttResult.ToString();
        }
    }
}
