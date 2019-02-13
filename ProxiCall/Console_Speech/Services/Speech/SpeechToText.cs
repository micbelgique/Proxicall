using System;
using System.IO;
using System.Threading.Tasks;
using Console_Speech.Services;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Console_Speech.Services.Speech
{
    class SpeechToText
    {

        public static async Task RecognizeSpeechFromBytesAsync(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);

            var speechApiKey = Environment.GetEnvironmentVariable("SpeechApiKey");
            var speechApiRegion = Environment.GetEnvironmentVariable("SpeechApiRegion");

            var speechConfig = SpeechConfig.FromSubscription(speechApiKey, speechApiRegion);

            var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
            var audioStream = new VoiceAudioStream(stream);
            var audioConfig = AudioConfig.FromStreamInput(audioStream, audioFormat);

            var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
            Console.WriteLine("Processing .wav file to text");
            var result = await recognizer.RecognizeOnceAsync();
            Console.WriteLine(".wav file reads : " + result.Text);
        }

        public static async Task RecognizeSpeechFromMicInputAsync()
        {
            var speechApiKey = Environment.GetEnvironmentVariable("SpeechApiKey");
            var speechApiRegion = Environment.GetEnvironmentVariable("SpeechApiRegion");

            var speechConfig = SpeechConfig.FromSubscription(speechApiKey, speechApiRegion);
            
            using (var recognizer = new SpeechRecognizer(speechConfig))
            {
                Console.WriteLine("Say something...");
                var result = await recognizer.RecognizeOnceAsync();
                
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine("You said : " + result.Text);
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }
    }
}
