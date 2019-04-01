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

namespace Proxicall.CRM.Controllers.Api
{
    [Authorize(Roles = "Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;
       
        public LeadsController(ProxicallCRMContext context)
        {
            _context = context;
        }

        [HttpGet("getopportunities")]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetAllOpportunitiesByLead(string firstname, string lastname)
        {
            var lead = await GetLeadByName(firstname, lastname);
            if (lead == null)
            {
                return BadRequest();
            }
            var opportunities = await _context.Opportunities
                .Where(o => o.Lead == lead)
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
            var lead = await GetLeadByName(firstName, lastName);
            if (lead == null)
            {
                return NotFound();
            }

            return lead;
        }

        private async Task<Lead> GetLeadByName(string firstName, string lastName)
        {
            firstName = char.ToLower(firstName[0]) + firstName.Substring(1).ToLower();
            lastName = char.ToLower(lastName[0]) + lastName.Substring(1).ToLower();
            var lead = await _context.Leads.Where(l =>
                l.FirstName == firstName && l.LastName == lastName
                ||
                l.FirstName == lastName && l.LastName == firstName)
            .Include(l => l.Company)
            .FirstOrDefaultAsync();

            if (lead == null)
            {
                return FindLeadWithLevenshtein(firstName,lastName);
            }

            return lead;
        }

        private Lead FindLeadWithLevenshtein(string firstName, string lastName)
        {
            var resultOfLvsDistanceDB = _context
                    .Leads
                    .Include(l => l.Company)
                    .LevenshteinDistanceOf(leadInDB => leadInDB.FirstName, leadInDB => leadInDB.LastName)
                    .ComparedTo(firstName, lastName);

            var allowedDistanceForFirstName = CalculateAllowedDistance(firstName);
            var allowedDistanceForLastName = CalculateAllowedDistance(lastName);

            foreach (var lvsDistance in resultOfLvsDistanceDB)
            {
                //"FirstName LastName" compared to "FirstName LastName" in the database
                var distanceFirstNameToFirstNameDB = lvsDistance.Distances[LevenshteinCompare.FirstNameToFirstNameDB.Id];
                var distanceLastNameToLastNameDB = lvsDistance.Distances[LevenshteinCompare.LastNameToLastNameDB.Id];

                if (distanceFirstNameToFirstNameDB <= allowedDistanceForFirstName
                    && distanceLastNameToLastNameDB <= allowedDistanceForLastName)
                {
                    return lvsDistance.Item;
                }
                //"FirstName LastName" compared to "LastName FirstName" in the database
                var distanceFirstNametoLastNameDB = lvsDistance.Distances[LevenshteinCompare.FirstNameToLastNameDB.Id];
                var distanceLastNameToFirstNameDB = lvsDistance.Distances[LevenshteinCompare.LastNameToFirstNameDB.Id];

                if (distanceFirstNametoLastNameDB <= allowedDistanceForFirstName
                    && distanceLastNameToFirstNameDB <= allowedDistanceForLastName)
                {
                    return lvsDistance.Item;
                }
            }
            return null;
        }
        
        private int CalculateAllowedDistance(string name)
        {
            if(name.Length<3)
            {
                return LevenshteinAllowedDistance.AllowedDistance.GetValueOrDefault(LevenshteinAllowedDistance.VERY_SMALL_WORD);
            }
            else if (name.Length<8)
            {
                return LevenshteinAllowedDistance.AllowedDistance.GetValueOrDefault(LevenshteinAllowedDistance.SMALL_WORD); ;
            }
            else
            {
                return LevenshteinAllowedDistance.AllowedDistance.GetValueOrDefault(LevenshteinAllowedDistance.MEDIUM_WORD); ;
            }
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
