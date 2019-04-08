using ProxiCall.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProxiCall.Services.ProxiCallCRM
{
    public class ProductService : BaseService
    {
        private readonly string _token;
        public ProductService(string token) : base()
        {
            _token = token;
        }

        public async Task<Product> GetProductByTitle(string title)
        {
            Product product = null;
            var path = $"api/products/byTitle?title={title}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<Product>();
            }
            return product;
        }
    }
}
