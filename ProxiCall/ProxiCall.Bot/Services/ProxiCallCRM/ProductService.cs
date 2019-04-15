using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ProxiCall.Bot.Exceptions.ProxiCallCRM;
using ProxiCall.Bot.Models;

namespace ProxiCall.Bot.Services.ProxiCallCRM
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiHost"));  
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
                case HttpStatusCode.Accepted:
                    var product = await response.Content.ReadAsAsync<Product>();
                    return product;
                case HttpStatusCode.Forbidden:
                    throw new AccessForbiddenException();
                case HttpStatusCode.NotFound:
                default:
                    throw new ProductNotFoundException();

            }
        }
    }
}
