using ProxiCall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class LeadService
    {
        private readonly HttpClient _httpClient;
        public LeadService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));
        }

        public async Task<Lead> GetLeadByName(string firstName, string lastName)
        {
            Lead lead = null;
            var path = $"api/leads/byName?firstName={firstName}&lastName={lastName}";
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
