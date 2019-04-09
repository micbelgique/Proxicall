using Newtonsoft.Json;
using ProxiCall.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace ProxiCall.Services.ProxiCallCRM
{
    public class OpportunityService : BaseService
    {
        private readonly string _token;
        public OpportunityService(string token) : base()
        {
            _token = token;
        }

        public async Task PostOpportunityAsync(OpportunityDetailed opportunity)
        {
            var path = $"api/opportunities";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            //TODO : research good practice
            var strictOpportunity = new Opportunity(opportunity);

            using (var request = new HttpRequestMessage(HttpMethod.Post, path))
            {
                var json = JsonConvert.SerializeObject(strictOpportunity);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await _httpClient
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}