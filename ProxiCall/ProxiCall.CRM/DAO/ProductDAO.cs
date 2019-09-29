using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Areas.Identity.Data;
using ProxiCall.CRM.Models;

namespace ProxiCall.CRM.DAO
{
    public class ProductDAO
    {
        private readonly ProxicallCRMContext _context;

        public ProductDAO(ProxicallCRMContext context)
        {
            _context = context;
        }
        public async Task<Product> GetProductByTitle(string title)
        {
            var product = await _context.Products.Where(p => p.Title == title)
                .FirstOrDefaultAsync();
            return product;
        }
    }
}
