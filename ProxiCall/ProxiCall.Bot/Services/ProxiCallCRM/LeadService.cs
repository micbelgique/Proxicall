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
    public class LeadService
    {
        private readonly HttpClient _httpClient;

        public LeadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));  
        }

        public async Task<Lead> GetLeadByName(string token, string firstName, string lastName)
        {
            if (token == null)
            {
                throw new InvalidTokenException("Token is null");
            }

            Lead lead = null;
            var path = $"api/leads/byName?firstName={firstName}&lastName={lastName}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(path);
            switch (response.StatusCode)
            {
                case HttpStatusCode.Accepted:
                    lead = await response.Content.ReadAsAsync<Lead>();
                    return lead;
                case HttpStatusCode.NotFound:
                default:
                    throw new LeadNotFoundException();
            }
        }

        public async Task<IEnumerable<OpportunityDetailed>> GetOpportunities(string token, string leadFirstName, string leadLastName, string ownerPhoneNumber)
        {
            if (token == null)
            {
                throw new InvalidTokenException("Token is null");
            }

            IEnumerable<OpportunityDetailed> opportunities = new List<OpportunityDetailed>();
            var path = $"api/leads/opportunities?leadfirstname={leadFirstName}&leadlastname={leadLastName}&ownerPhoneNumber={ownerPhoneNumber}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(path);
            switch (response.StatusCode)
            {
                case HttpStatusCode.Accepted:
                    opportunities = await response.Content.ReadAsAsync<IEnumerable<OpportunityDetailed>>();
                    return opportunities;
                case HttpStatusCode.Forbidden:
                    throw new AccessForbiddenException();
                case HttpStatusCode.NotFound:
                    switch (response.ReasonPhrase)
                    {
                        case "lead-not-found":
                            throw new LeadNotFoundException();
                        case "owner-not-found":
                            throw new OwnerNotFoundException();
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
