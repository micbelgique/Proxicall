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
        public static async Task<Product> GetProductByTitle(ProxicallCRMContext context, string title)
        {
            var product = await context.Products.Where(p => p.Title == title)
                .FirstOrDefaultAsync();
            return product;
        }
    }
}
