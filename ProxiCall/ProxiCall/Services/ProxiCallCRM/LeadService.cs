using ProxiCall.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class LeadService : BaseService
    {
        public LeadService(string token)
            : base(token)
        {
        }

        public async Task<Lead> GetLeadByName(string firstName, string lastName)
        {
            Lead lead = null;
            var path = $"api/leads/byName?firstName={firstName}&lastName={lastName}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                lead = await response.Content.ReadAsAsync<Lead>();
                return lead;
            }
            return null;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunities(string leadFirstName, string leadLastName, string ownerPhoneNumber)
        {
            IEnumerable<Opportunity> opportunities = new List<Opportunity>();
            var path = $"api/leads/opportunities?leadfirstname={leadFirstName}&leadlastname={leadLastName}&ownerPhoneNumber={ownerPhoneNumber}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                opportunities = await response.Content.ReadAsAsync<IEnumerable<Opportunity>>();
                return opportunities;
            }
            return null;
        }
    }
}
