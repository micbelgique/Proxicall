﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Areas.Identity.Data;
using ProxiCall.CRM.DAO;
using ProxiCall.CRM.Models;

namespace ProxiCall.CRM.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + MicrosoftAccountDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;
        private readonly LeadDAO _leadDao;

        public LeadsController(ProxicallCRMContext context, LeadDAO leadDAO)
        {
            _context = context;
            _leadDao = leadDAO;
        }
        
        [HttpGet("allnames")]
        public async Task<ActionResult<IEnumerable<string>>> GetLeadsNames()
        {
            var leads = await _context.Leads.ToListAsync();
            var names = new List<string>();
            foreach(var lead in leads)
            {
                names.Add(lead.FullName);
            }
            return names;
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
                return BadRequest();
            }

            //Searching the lead
            var lead = await _leadDao.GetLeadByName(leadFirstName, leadLastName);
            if (lead == null)
            {
                return NotFound("lead-not-found");
            }

            //Searching the owner
            var owner = _context.Set<ApplicationUser>().FirstOrDefault(u => u.PhoneNumber == ownerPhoneNumber);
            if (owner == null)
            {
                return NotFound("owner-not-found");
            }

            var opportunities = await _context.Opportunities
                .Where(o => o.Lead == lead && o.Owner == owner)
                .Include(o => o.Owner)
                .Include(o => o.Product)
                .Include(o => o.Lead)
                .ToListAsync();

            if (opportunities == null || opportunities.Count == 0)
            {
                return NotFound("opportunities-not-found");
            }

            return opportunities;
        }
        [HttpGet("byName")]
        public async Task<ActionResult<Lead>> GetLead(string firstName, string lastName)
        {
            Lead lead = null;
            if(lastName == null)
            {
                lead = await _context.Leads.FirstOrDefaultAsync(x => x.FirstName == firstName || x.LastName == firstName);
            }
            else
            {
                lead = await _leadDao.GetLeadByName(firstName, lastName);
            }
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
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == lead.CompanyId);
            company.ContactId = lead.Id;
            _context.Entry(lead).State = EntityState.Modified;
            _context.Entry(company).State = EntityState.Modified;
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
