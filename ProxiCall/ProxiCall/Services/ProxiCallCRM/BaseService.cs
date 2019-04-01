using Microsoft.Bot.Builder;
using System;
using System.Net.Http;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class BaseService
    {
        protected readonly HttpClient _httpClient;

        protected string AuthToken { get; set; } = string.Empty;

        public BaseService()
        {

        }

        public BaseService(string token)
        {
            AuthToken = token;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));
        }
    }
}
