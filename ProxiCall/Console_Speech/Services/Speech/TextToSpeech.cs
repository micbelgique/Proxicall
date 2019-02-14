using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Console_Speech.Services.Speech
{
    class TextToSpeech
    {
        private static readonly string host = "https://westeurope.tts.speech.microsoft.com/cognitiveservices/v1";

        public static async Task<byte[]> TransformTextToSpeechAsync(string texttotransform, string locale)
        {
            // Gets an access token
            string accessToken;

            // Add your subscription key here
            // If your resource isn't in WEST US, change the endpoint
            AuthentificationSpeechApi auth = new AuthentificationSpeechApi(Environment.GetEnvironmentVariable("SpeechApiKey"));
            accessToken = auth.GetAccessToken();

            // Set request body
            string body = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='" + locale + "'>" +
                                "<voice name='Microsoft Server Speech Text to Speech Voice (en-US, JessaNeural)'>" +
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
                    request.RequestUri = new Uri(host);
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
    }
}
