using System;
using System.Net.Http;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class BaseService
    {
        protected readonly HttpClient _httpClient;

        protected string AuthToken { get; set; } = string.Empty;

        public BaseService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));
        }

        public BaseService(string token) : base()
        {
            AuthToken = token;
        }
    }
}
