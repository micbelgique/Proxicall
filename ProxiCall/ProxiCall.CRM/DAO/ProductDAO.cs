using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proxicall.CRM.DAO
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
