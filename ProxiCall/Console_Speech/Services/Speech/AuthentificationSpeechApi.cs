using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Console_Speech.Services.Speech
{
    class AuthentificationSpeechApi
    {
        public static readonly string AccessUriSpeech = "https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issuetoken";

        private readonly string _subscriptionKey;
        private readonly string _token;

        public AuthentificationSpeechApi(string subscriptionKey)
        {
            this._subscriptionKey = subscriptionKey;
            this._token = FetchTokenAsync(AccessUriSpeech, subscriptionKey).Result;
        }

        public string GetAccessToken()
        {
            return this._token;
        }

        private async Task<string> FetchTokenAsync(string fetchUri, string subscriptionKey)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                UriBuilder uriBuilder = new UriBuilder(fetchUri);

                var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
                Console.WriteLine("Token Uri: {0}", uriBuilder.Uri.AbsoluteUri);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
