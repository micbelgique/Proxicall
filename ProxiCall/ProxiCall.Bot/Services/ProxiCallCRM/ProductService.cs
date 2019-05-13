using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;
using ProxiCall.Bot.Models.AppSettings;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ServicesConfig _servicesConfig;

        public ProductService(HttpClient httpClient, IOptions<ServicesConfig> options)
        {
            _servicesConfig = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_servicesConfig.ProxiCallCrmHostname);   
        }

        public async Task<Product> GetProductByTitle(string token, string title)
        {
            if (token == null)
            {
                throw new InvalidTokenException("Token is null");
            }

            var path = $"api/products/byTitle?title={title}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(path);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var product = await response.Content.ReadAsAsync<Product>();
                    return product;
                case HttpStatusCode.Forbidden:
                    throw new AccessForbiddenException();
                case HttpStatusCode.NotFound:
                default:
                    return null;
            }
        }
    }
}
