using ProxiCall.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class LeadService : BaseService
    {
        private readonly string _token;
        public LeadService(string token) : base()
        {
            _token = token;
        }

        public async Task<Lead> GetLeadByName(string firstName, string lastName)
        {
            Lead lead = null;
            var path = $"api/leads/byName?firstName={firstName}&lastName={lastName}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                lead = await response.Content.ReadAsAsync<Lead>();
            }
            return lead;
        }

        public async Task<IEnumerable<OpportunityDetailed>> GetOpportunities(string leadFirstName, string leadLastName, string ownerPhoneNumber)
        {
            IEnumerable<OpportunityDetailed> opportunities = new List<OpportunityDetailed>();
            var path = $"api/leads/opportunities?leadfirstname={leadFirstName}&leadlastname={leadLastName}&ownerPhoneNumber={ownerPhoneNumber}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                opportunities = await response.Content.ReadAsAsync<IEnumerable<OpportunityDetailed>>();
                return opportunities;
            }
            return null;
        }
    }
}
