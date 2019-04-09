using ProxiCall.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class CompanyService : BaseService
    {
        private readonly string _token;
        public CompanyService(string token) : base()
        {
            _token = token;
        }

        public async Task<Company> GetCompanyByName(string name)
        {
            var path = $"api/companies/byName?name={name}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<Company>();
            }
            return null;
        }

        public async Task<IEnumerable<OpportunityDetailed>> GetOpportunities(string companyName, string ownerPhoneNumber)
        {
            IEnumerable<OpportunityDetailed> opportunities = new List<OpportunityDetailed>();
            var path = $"api/companies/opportunities?companyName={companyName}&ownerPhoneNumber={ownerPhoneNumber}";
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
