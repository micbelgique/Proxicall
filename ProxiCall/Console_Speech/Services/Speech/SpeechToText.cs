using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Console_Speech.Services.Speech
{
    class SpeechToText
    {
        public static async Task<string> RecognizeSpeechFromBytesAsync(byte[] bytes, string locale)
        {
            Console.WriteLine("Processing .wav file to text");
            MemoryStream stream = new MemoryStream(bytes);

            var speechApiKey = Environment.GetEnvironmentVariable("SpeechApiKey");
            var speechApiRegion = Environment.GetEnvironmentVariable("SpeechApiRegion");

            var speechConfig = SpeechConfig.FromSubscription(speechApiKey, speechApiRegion);
            speechConfig.SpeechRecognitionLanguage = locale;

            var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
            var audioStream = new VoiceAudioStream(stream);
            var audioConfig = AudioConfig.FromStreamInput(audioStream, audioFormat);

            var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
            var result = await recognizer.RecognizeOnceAsync();
            return result.Text;
        }

        public static async Task<string> RecognizeSpeechFromMicInputAsync(string locale)
        {
            var speechApiKey = Environment.GetEnvironmentVariable("SpeechApiKey");
            var speechApiRegion = Environment.GetEnvironmentVariable("SpeechApiRegion");

            var speechConfig = SpeechConfig.FromSubscription(speechApiKey, speechApiRegion);
            speechConfig.SpeechRecognitionLanguage = locale;

            using (var recognizer = new SpeechRecognizer(speechConfig))
            {
                StringBuilder builder = new StringBuilder(string.Empty);
                Console.WriteLine("Say something...");
                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    //Append the result when the "speechtotext" conversion succeed
                    builder.AppendLine(result.Text);
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    builder.AppendLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    builder.AppendLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        builder.AppendLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        builder.AppendLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        builder.AppendLine($"CANCELED: Did you update the subscription info?");
                    }
                }
                return builder.ToString();
            }
        }
    }
}
