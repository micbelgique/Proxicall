﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Models;
using Proxicall.CRM.Models;

namespace Proxicall.CRM.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;

        public LeadsController(ProxicallCRMContext context)
        {
            _context = context;
        }

        [HttpGet("allOpportunities")]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetAllOpportunitiesByLead(string firstname, string lastname)
        {
            var lead = await GetLeadByName(firstname, lastname);
            if (lead == null)
            {
                return BadRequest();
            }
            var opportunities = await _context.Opportunity
                .Where(o => o.Lead == lead)
                .Include(o => o.Owner)
                .Include(o => o.Product)
                .Include(o => o.Lead)
                .ToListAsync();

            if (opportunities == null)
            {
                return NotFound();
            }

            return opportunities;
        }

        [HttpGet("byName")]
        public async Task<ActionResult<Lead>> GetLead(string firstName, string lastName)
        {
            
        }

        private async Task<Lead> GetLeadByName(string firstName, string lastName)
        {
            firstName = char.ToLower(firstName[0]) + firstName.Substring(1).ToLower();
            lastName = char.ToLower(lastName[0]) + lastName.Substring(1).ToLower();
            var lead = await _context.Lead.Where(l =>
                l.FirstName == firstName && l.LastName == lastName
                ||
                l.FirstName == lastName && l.LastName == firstName)
            .Include(l => l.Company)
            .FirstOrDefaultAsync();

            if (lead == null)
            {
                return null;
            }

            return lead;
        }

        // GET: api/Leads
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lead>>> GetLead()
        {
            return await _context.Lead.Include(l => l.Company).ToListAsync();
        }

        // GET: api/Leads/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lead>> GetLead(string id)
        {
            var lead = await _context.Lead.Include(l => l.Company).FirstOrDefaultAsync(l => l.Id == id);

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
            _context.Lead.Add(lead);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLead", new { id = lead.Id }, lead);
        }

        // DELETE: api/Leads/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Lead>> DeleteLead(string id)
        {
            var lead = await _context.Lead.Include(l => l.Company).FirstOrDefaultAsync(l => l.Id == id);
            if (lead == null)
            {
                return NotFound();
            }

            _context.Lead.Remove(lead);
            await _context.SaveChangesAsync();

            return lead;
        }

        private bool LeadExists(string id)
        {
            return _context.Lead.Any(e => e.Id == id);
        }
    }
}
