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

        public async Task<User> Authenticate(string phonenumber)
        {
            var path = $"api/account/login?phoneNumber={phonenumber}";
            var response = await _httpClient.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<LoginDTO>();
                var user = new User();
                user.UserName = result.UserName;
                user.Alias = result.UserName.Split('@')[0];
                user.Token = result.Token;
                return user;
            }
            return null;
        }
    }
}
