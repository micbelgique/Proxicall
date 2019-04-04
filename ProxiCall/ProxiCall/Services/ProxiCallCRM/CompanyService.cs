using ProxiCall.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class CompanyService : BaseService
    {
        public CompanyService(string token) : base(token)
        {
        }

        public async Task<Company> GetCompanyByName(string name)
        {
            var path = $"api/companies/byName?name={name}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<Company>();
            }
            return null;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunities(string companyName, string ownerPhoneNumber)
        {
            IEnumerable<Opportunity> opportunities = new List<Opportunity>();
            var path = $"api/companies/opportunities?companyName={companyName}&ownerPhoneNumber={ownerPhoneNumber}";
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
