using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProxiCall.Web.Models.AppSettings;

namespace ProxiCall.Web.Services.ProxiCallCRM
{
    public class NamesService
    {
        private readonly HttpClient _httpClient;
        private readonly DirectlineConfig _directlineConfig;

        public NamesService(HttpClient httpClient, IOptions<DirectlineConfig> directlineOptions)
        {
            _httpClient = httpClient;
            _directlineConfig = directlineOptions.Value;
            _httpClient.BaseAddress = _directlineConfig.ProxiCallCrmHostname;
        }

        private async Task<string> GetAuthToken()
        {
            var path = $"api/account/login?credential={_directlineConfig.AdminPhoneNumber}&loginMethod=phone";
            var response = await _httpClient.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadAsAsync<User>();
                return user.Token;
            }

            throw new Exception(response.ReasonPhrase);
        }

        private string StringArrayToString(string[] strings)
        {
            var sb = new StringBuilder();
            foreach (var str in strings)
            {
                sb.Append(str);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public async Task<string> FetchNamesFromCrm()
        {
            List<string> allNames = new List<string>();
            var token = await GetAuthToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string[] apiControllers = {"leads", "companies", "products"};
            foreach (var controller in apiControllers)
            {
                var path = $"api/{controller}/allnames";
                var response = await _httpClient.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    allNames.AddRange(await response.Content.ReadAsAsync<List<string>>());
                }
            }

            return StringArrayToString(allNames.ToArray());
        }

        private class User
        {
            public string Token { get; set; }
        }
    }
}
