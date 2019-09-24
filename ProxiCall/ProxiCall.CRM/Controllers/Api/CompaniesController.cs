using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Areas.Identity.Data;
using ProxiCall.CRM.DAO;
using ProxiCall.CRM.Models;

namespace ProxiCall.CRM.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;
        private readonly CompanyDAO _companyDAO;

        public CompaniesController(ProxicallCRMContext context, CompanyDAO companyDAO)
        {
            _context = context;
            _companyDAO = companyDAO;
        }

        [HttpGet("byName")]
        public async Task<ActionResult<Company>> GetFullCompanyByName(string name)
        {
            var company = await _companyDAO.GetCompanyByName(name);
            if (company == null)
            {
                return BadRequest();
            }

            return company;
        }

        [HttpGet("getcontact")]
        public async Task<ActionResult<Lead>> GetContact(string name)
        {
            var company = await _companyDAO.GetCompanyByName(name);
            if (company == null)
            {
                return BadRequest();
            }

            if (company.Contact == null)
            {
                return NotFound();
            }
            return company.Contact;
        }

        [HttpGet("opportunities")]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetOpportunitiesByCompanyAndOwner
            (string companyName,string ownerPhoneNumber)
        {
            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(ownerPhoneNumber))
            {
                return BadRequest();
            }

            //Searching the company
            var company = await _companyDAO.GetCompanyByName(companyName);
            if (company == null)
            {
                return NotFound("company-not-found");
            }

            //Searching the owner
            var owner = _context.Set<ApplicationUser>().FirstOrDefault(u => u.PhoneNumber == ownerPhoneNumber);
            if (owner == null)
            {
                return  NotFound("owner-not-found");
            }
            
            //Searching the opportunities
            var opportunities = await _context.Opportunities
                .Include(o => o.Owner)
                .Include(o => o.Product)
                .Include(o => o.Lead)
                .Where(o => o.Lead.Company == company && o.Owner == owner)
                .ToListAsync();

            if (opportunities == null || opportunities.Count == 0)
            {
                return NotFound("opportunities-not-found");
            }
            return Ok(opportunities);
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompany()
        {
            var companies = await _context.Companies.ToListAsync();
            return companies;
        }

        [HttpGet("allnames")]
        public async Task<ActionResult<IEnumerable<string>>> GetCompanyNames()
        {
            var companies = await _context.Companies.Include(c => c.Contact).ToListAsync();
            var names = new List<string>();
            foreach(var company in companies)
            {
                names.Add(company.Name);
            }
            return names;
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(string id)
        {
            var company = await _context.Companies.Include(c => c.Contact).FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        // PUT: api/Companies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(string id, Company company)
        {
            if (id != company.Id)
            {
                return BadRequest();
            }

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Companies
        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Company>> DeleteCompany(string id)
        {
            var company = await _context.Companies.Include(c => c.Contact).FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return company;
        }

        private bool CompanyExists(string id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
