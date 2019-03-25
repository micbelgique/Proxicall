using Google.Cloud.Speech.V1;
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
            RecognitionConfig config = new RecognitionConfig
            {
                Encoding = AudioEncoding.Linear16,
                SampleRateHertz = 8000,
                LanguageCode = LanguageCodes.French.France,
                Model = "phone_call"
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
