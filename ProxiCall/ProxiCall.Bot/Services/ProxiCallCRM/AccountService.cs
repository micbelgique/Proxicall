using System.Net.Http;
using System.Threading.Tasks;
using ProxiCall.Bot.Models;

namespace ProxiCall.Bot.Services.ProxiCallCRM
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
                var user = await response.Content.ReadAsAsync<User>();
                return user;
            }
            return null;
        }
    }
}
