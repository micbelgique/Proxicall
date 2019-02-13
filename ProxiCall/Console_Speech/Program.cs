using Console_Speech.Services.Speech;
using System;
using System.IO;

namespace Console_Speech
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            byte[] wav = File.ReadAllBytes(Path.Combine(path, $"Resources", $"whatstheweatherlike.wav"));
            SpeechToText.RecognizeSpeechFromMicInputAsync().Wait();
            SpeechToText.RecognizeSpeechFromBytesAsync(wav).Wait();
            Console.WriteLine("Please press a key to continue.");
            Console.ReadLine();

            Stream result_tts = TextToSpeech.TransformTextToSpeechAsync("Test of Text To Speech in console", "en-US").Result;

        }
    }
}
