using ProxiCall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class CompanyService
    {
        private readonly HttpClient _httpClient;
        public CompanyService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));
        }

        public async Task<Company> GetCompanyByName(string name)
        {
            var path = $"api/companies/byName?name={name}";
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<Company>();
            }
            return null;
        }
    }
}
