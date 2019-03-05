using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxiCall.Web.Services.Speech
{
    class AuthentificationApi
    {
        private readonly string _subscriptionKey;
        private readonly string _token;

        public AuthentificationApi(string subscriptionKey, string region = "westeurope")
        {
            StringBuilder apiURIBuilder = new StringBuilder("https://");
            apiURIBuilder.Append(region).Append(".api.cognitive.microsoft.com/sts/v1.0/issuetoken");

            _subscriptionKey = subscriptionKey;
            _token = FetchTokenAsync(apiURIBuilder.ToString(), subscriptionKey).Result;
        }

        public string GetAccessToken()
        {
            return _token;
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
