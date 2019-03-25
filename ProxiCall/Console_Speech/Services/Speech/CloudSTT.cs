using Google.Cloud.Speech.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Console_Speech.Services.Speech
{
    class CloudSTT
    {
        public static string RecognizeSpeechFromWav(string wavPath)
        {
            var speech = SpeechClient.Create();
            var response = speech.Recognize(new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 8000,
                LanguageCode = "fr",
            }, RecognitionAudio.FromFile(wavPath));
            var str = "";
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    str += alternative.Transcript;
                }
            }
            return str;
        }
    }
}
