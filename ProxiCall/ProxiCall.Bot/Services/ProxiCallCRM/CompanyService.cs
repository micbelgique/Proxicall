using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class CompanyService
    {
        private readonly HttpClient _httpClient;

        public CompanyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));  
        }

        public async Task<Company> GetCompanyByName(string token, string name)
        {
            if (token == null)
            {
                throw new InvalidTokenException("Token is null");
            }

            var path = $"api/companies/byName?name={name}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<Company>();
            }
            return null;
        }

        public async Task<IEnumerable<OpportunityDetailed>> GetOpportunities(string token, string companyName, string ownerPhoneNumber)
        {
            if (token == null)
            {
                throw new InvalidTokenException("Token is null");
            }

            IEnumerable<OpportunityDetailed> opportunities = new List<OpportunityDetailed>();
            var path = $"api/companies/opportunities?companyName={companyName}&ownerPhoneNumber={ownerPhoneNumber}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(path);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    opportunities = await response.Content.ReadAsAsync<IEnumerable<OpportunityDetailed>>();
                    return opportunities;
                case HttpStatusCode.Forbidden:
                    throw new AccessForbiddenException();
                case HttpStatusCode.NotFound:
                    switch (response.ReasonPhrase)
                    {
                        case "owner-not-found":
                            throw new OwnerNotFoundException();
                        case "company-not-found":
                            throw new CompanyNotFoundException();
                        case "opportunities-not-found":
                        default:
                            throw new OpportunitiesNotFoundException();
                    }
                case HttpStatusCode.BadRequest:
                default:
                    throw new OpportunitiesNotFoundException();
            }
        }
    }
}
