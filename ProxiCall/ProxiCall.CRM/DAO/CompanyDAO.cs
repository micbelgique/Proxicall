using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Proxicall.CRM.DAO
{
    public class CompanyDAO
    {
        private readonly ProxicallCRMContext _context;

        public CompanyDAO(ProxicallCRMContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompanyByName(string name)
        {
            var company = await _context.Companies.Where(c => c.Name == name)
                .Include(c => c.Contact)
                .FirstOrDefaultAsync();
            return company;
        }
    }
}
