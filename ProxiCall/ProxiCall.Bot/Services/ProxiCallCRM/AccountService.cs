using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Models.AppSettings;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;
        private readonly ServicesConfig _servicesConfig;

        public AccountService(HttpClient httpClient, IOptions<ServicesConfig> options)
        {
            _servicesConfig = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_servicesConfig.ProxiCallCrmHostname);   
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
