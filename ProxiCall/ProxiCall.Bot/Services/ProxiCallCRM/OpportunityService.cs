using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Models.AppSettings;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class OpportunityService
    {
        private readonly HttpClient _httpClient;
        private readonly ServicesConfig _servicesConfig;

        public OpportunityService(HttpClient httpClient, IOptions<ServicesConfig> options)
        {
            _servicesConfig = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_servicesConfig.ProxiCallCrmHostname);   
        }

        public async Task PostOpportunityAsync(string token, OpportunityDetailed opportunity)
        {
            if (token == null)
            {
                throw new InvalidTokenException("Token is null");
            }

            var path = $"api/opportunities";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.Created:
                                break;
                            case HttpStatusCode.Forbidden:
                                throw new AccessForbiddenException();
                            default:
                                throw new OpportunityNotCreatedException();
                        }
                    }
                }
            }
        }
    }
}