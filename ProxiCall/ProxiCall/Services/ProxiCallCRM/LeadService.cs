using ProxiCall.Models;
using System;
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
    }
}
