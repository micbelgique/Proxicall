using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Models.AppSettings;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class LeadService
    {
        private readonly HttpClient _httpClient;
        private readonly ServicesConfig _servicesConfig;

        public LeadService(HttpClient httpClient, IOptions<ServicesConfig> options)
        {
            _servicesConfig = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_servicesConfig.ProxiCallCrmHostname);   
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
                case HttpStatusCode.OK:
                    lead = await response.Content.ReadAsAsync<Lead>();
                    return lead;
                case HttpStatusCode.Forbidden:
                    throw new AccessForbiddenException();
                default:
                    return null;
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
                case HttpStatusCode.OK:
                    opportunities = await response.Content.ReadAsAsync<IEnumerable<OpportunityDetailed>>();
                    return opportunities;
                case HttpStatusCode.Forbidden:
                    throw new AccessForbiddenException();
                default:
                    return null;
            }
        }
    }
}
