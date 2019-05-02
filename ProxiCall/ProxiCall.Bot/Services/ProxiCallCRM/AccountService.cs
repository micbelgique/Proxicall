using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));   
        }

        public async Task<User> Authenticate(string credential, string loginMethod = "phone") 
        {
            var path = $"api/account/login?credential={credential}&loginMethod={loginMethod}";
            var response = await _httpClient.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadAsAsync<User>();
                return user;
            }
            throw new UserNotFoundException($"Status code : {response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}
