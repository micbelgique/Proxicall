using ProxiCall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxiCall.Services
{
    public class ProxicallDAO
    {
        private readonly HttpClient _httpClient;
        public ProxicallDAO()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));
        }

        public async Task<string> GetPhoneNumberByFirstName(string firstname)
        {
            User user = null;
            var path = $"api/users/phonenumber/{firstname}";
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                user = await response.Content.ReadAsAsync<User>();
                return user.PhoneNumber;
            }
            return null;
        }
    }
}
