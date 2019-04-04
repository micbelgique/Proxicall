using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;
using NinjaNye.SearchExtensions.Levenshtein;
using Proxicall.CRM.Models.Enumeration.Levenshtein;
using Proxicall.CRM.Models.Dictionnaries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Proxicall.CRM.DAO;
using Microsoft.AspNetCore.Identity;

namespace Proxicall.CRM.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;
       
        public LeadsController(ProxicallCRMContext context)
        {
            _context = context;
        }
        
        // GET: api/Leads
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lead>>> GetLead()
        {
            return await _context.Leads.Include(l => l.Company).ToListAsync();
        }

        // GET: api/Leads/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lead>> GetLead(string id)
        {
            var lead = await _context.Leads.Include(l => l.Company).FirstOrDefaultAsync(l => l.Id == id);

            if (lead == null)
            {
                return NotFound();
            }

            return lead;
        }

        [HttpGet("opportunities")]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetOpportunitiesByLeadAndOwner
            (string leadFirstName, string leadLastName, string ownerPhoneNumber)
        {
            if(string.IsNullOrEmpty(leadFirstName) || string.IsNullOrEmpty(leadLastName) || string.IsNullOrEmpty(ownerPhoneNumber))
            {
                return NotFound();
            }

            //Searching the lead
            var lead = await LeadDAO.GetLeadByName(_context, leadFirstName, leadLastName);
            if (lead == null)
            {
                return NotFound();
            }

            //Searching the owner
            var owner = _context.Set<IdentityUser>().FirstOrDefault(u => u.PhoneNumber == ownerPhoneNumber);
            if (owner == null)
            {
                return NotFound();
            }

            var opportunities = await _context.Opportunities
                .Where(o => o.Lead == lead && o.Owner == owner)
                .Include(o => o.Owner)
                .Include(o => o.Product)
                .Include(o => o.Lead)
                .ToListAsync();

            if (opportunities == null || opportunities.Count == 0)
            {
                return NotFound();
            }

            return opportunities;
        }

        [HttpGet("byName")]
        public async Task<ActionResult<Lead>> GetLead(string firstName, string lastName)
        {
            var lead = await LeadDAO.GetLeadByName(_context, firstName, lastName);
            if (lead == null)
            {
                return NotFound();
            }

            return lead;
        }

        // PUT: api/Leads/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLead(string id, Lead lead)
        {
            if (id != lead.Id)
            {
                return BadRequest();
            }

            _context.Entry(lead).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeadExists(id))
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

        // POST: api/Leads
        [HttpPost]
        public async Task<ActionResult<Lead>> PostLead(Lead lead)
        {
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLead", new { id = lead.Id }, lead);
        }

        // DELETE: api/Leads/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Lead>> DeleteLead(string id)
        {
            var lead = await _context.Leads.Include(l => l.Company).FirstOrDefaultAsync(l => l.Id == id);
            if (lead == null)
            {
                return NotFound();
            }

            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();

            return lead;
        }

        private bool LeadExists(string id)
        {
            return _context.Leads.Any(e => e.Id == id);
        }
    }
}
