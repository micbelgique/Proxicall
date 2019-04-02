using ProxiCall.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class AccountService : BaseService
    {
        public AccountService() : base ()
        {
        }

        public async Task<LoginDTO> Authenticate(string phonenumber)
        {
            var path = $"api/account/login?phoneNumber={phonenumber}";
            var response = await _httpClient.GetAsync(path);
            var result = await response.Content.ReadAsAsync<LoginDTO>();
            return result;
        }
    }
}
