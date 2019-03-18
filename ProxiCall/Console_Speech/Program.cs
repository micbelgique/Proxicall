using Console_Speech.Services;
using Console_Speech.Services.Speech;
using System;
using System.IO;

namespace Console_Speech
{
    class Program
    {
        static void Main(string[] args)
        {

            //Console.WriteLine(SpeechToText.RecognizeSpeechFromMicInputAsync("fr-FR").Result);

            var path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;

            //wav = File.ReadAllBytes(Path.Combine(path, $"Resources", $"whatstheweatherlike.wav"));
            //Console.WriteLine(SpeechToText.RecognizeSpeechFromBytesAsync(wav, "en-US").Result);

            //byte[] result_tts = TextToSpeech.TransformTextToSpeechAsync("This is a test in english.", "en-US").Result;
            //FormatConvertor.TurnAudioStreamToFile(result_tts, path).Wait();
            //Console.WriteLine(SpeechToText.RecognizeSpeechFromBytesAsync(result_tts, "en-US").Result);

            //byte[] result_tts = TextToSpeech.TransformTextToSpeechAsync("Voici le test en français.", "fr-FR").Result;
            //FormatConvertor.TurnAudioStreamToFile(result_tts, path).Wait();
            //Console.WriteLine(SpeechToText.RecognizeSpeechFromBytesAsync(result_tts, "fr-FR").Result);

            //string uriDream = "http://s1download-universal-soundbank.com/wav/7734.wav";
            //string uri = "https://api.twilio.com/2010-04-01/Accounts/AC5063b64f7001fdb7ea4656e698b9bb3a/Recordings/RE51f4a3b79ea9d4f9b2014289f6852ff0.wav";
            string uriWeather = "https://raw.githubusercontent.com/Microsoft/ProjectOxford-ClientSDK/master/Speech/SpeechToText/Windows/samples/SpeechRecognitionServiceExample/whatstheweatherlike.wav";
            Console.WriteLine(SpeechToText.RecognizeSpeechFromUrlAsync(uriWeather, "en-US").Result);

            Console.WriteLine("Please press a key to continue.");
            Console.ReadLine();

        }
    }
}
