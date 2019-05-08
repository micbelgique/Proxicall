using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxiCall.Web.Services.Speech
{
    class TextToSpeech
    {
        public async Task<byte[]> TransformTextToSpeechAsync
            (string texttotransform, string locale, string region = "westeurope")
        {
            string accessToken;
            // If your resource isn't in WEST EUROPE, change the endpoint (ex: "westus")
            AuthentificationApi auth = new AuthentificationApi(Environment.GetEnvironmentVariable("SpeechApiKey"), region);
            accessToken = auth.GetAccessToken();

            var voiceName = ChoseProperVoice();
            // Set request body
            string body = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='" + locale + "'>" +
                                "<voice name='Microsoft Server Speech Text to Speech Voice (" + locale + ", " + voiceName + ")'>" +
                                    texttotransform +
                                "</voice>" +
                            "</speak>";

            // Http request
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Set the HTTP method
                    request.Method = HttpMethod.Post;

                    // Construct the URI
                    StringBuilder requestURIBuilder = new StringBuilder("https://");
                    requestURIBuilder.Append(region).Append(".tts.speech.microsoft.com/cognitiveservices/v1");
                    request.RequestUri = new Uri(requestURIBuilder.ToString());

                    // Set the content type header
                    request.Content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");

                    // Set additional header, such as Authorization and User-Agent
                    request.Headers.Add("Authorization", "Bearer " + accessToken);
                    request.Headers.Add("Connection", "Keep-Alive");

                    // Update your resource name
                    request.Headers.Add("User-Agent", "ProxiCallSpeech");
                    request.Headers.Add("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");

                    // Create a request
                    using (var response = await httpClient.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var audioResponse = await response.Content.ReadAsByteArrayAsync();
                        return audioResponse;
                    }
                }
            }
        }

        private string ChoseProperVoice()
        {
            var acceptedCultureNames = new string[]
            {
                "en",
                "fr",
                "fr-FR",
                "fr-CA",
                "en-US",
                "en-UK"
            };

            if (!acceptedCultureNames.Contains(CultureInfo.CurrentCulture.Name))
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
            }

            string cultureName = CultureInfo.CurrentCulture.Name;
            if (cultureName.Substring(0, 2) == "fr")
            {
                return "Julie, Apollo";
            }
            else
            {
                return "JessaNeural";
            }
        }
    }
}
