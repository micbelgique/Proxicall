using Console_Speech.Services;
using Console_Speech.Services.Speech;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Console_Speech
{
    class Program
    {
        static async Task<string> TestSpeech()
        {
            byte[] result_tts = await TextToSpeech.TransformTextToSpeechAsync("This is a test in english.", "en-US");
            var str = await SpeechToText.RecognizeSpeechFromBytesAsync(result_tts, "en-US");
            return str;
        }
        static void Main(string[] args)
        {
            var path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            //byte[] wav = File.ReadAllBytes(Path.Combine(path, $"Resources", $"whatstheweatherlike.wav"));
            //var str = SpeechToText.RecognizeSpeechFromMicInputAsync("fr-FR").Result;
            //Console.WriteLine(str);
            //var str2 = SpeechToText.RecognizeSpeechFromBytesAsync(wav,"en-US").Result;
            //Console.WriteLine(str2);

            var str = TestSpeech().Result;
            Console.WriteLine("Console : " + str);

            //result_tts = TextToSpeech.TransformTextToSpeechAsync("Je m'appelle Julie et j'adore le php", "fr-FR").Result;
            //FormatConvertor.TurnAudioStreamToFile(result_tts, path).Wait();
            Console.WriteLine("Please press a key to continue.");
            Console.ReadLine();

        }
    }
}
