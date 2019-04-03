using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Proxicall.CRM.DAO
{
    public class CompanyDAO
    {
        public static async Task<Company> GetCompanyByName(ProxicallCRMContext context, string name)
        {
            var company = await context.Companies.Where(c => c.Name == name)
                .Include(c => c.Contact)
                .FirstOrDefaultAsync();
            if (company == null)
            {
                return null;
            }

            return company;
        }
    }
}
