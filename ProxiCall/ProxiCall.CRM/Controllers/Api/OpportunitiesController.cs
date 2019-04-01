using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;

namespace Proxicall.CRM.Controllers.Api
{
    [Authorize(Roles = "Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class OpportunitiesController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;

        public OpportunitiesController(ProxicallCRMContext context)
        {
            _context = context;
        }

        // GET: api/Opportunities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetOpportunity()
        {
            return await _context.Opportunities.Include(o => o.Owner).Include(o => o.Product).Include(o => o.Lead).ToListAsync();
        }

        // GET: api/Opportunities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Opportunity>> GetOpportunity(string id)
        {
            var opportunity = await _context.Opportunities.Include(o => o.Owner).Include(o => o.Product).Include(o => o.Lead).FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
            {
                return NotFound();
            }

            return opportunity;
        }

        // PUT: api/Opportunities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOpportunity(string id, Opportunity opportunity)
        {
            if (id != opportunity.Id)
            {
                return BadRequest();
            }

            _context.Entry(opportunity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OpportunityExists(id))
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

        // POST: api/Opportunities
        [HttpPost]
        public async Task<ActionResult<Opportunity>> PostOpportunity(Opportunity opportunity)
        {
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOpportunity", new { id = opportunity.Id }, opportunity);
        }

        // DELETE: api/Opportunities/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Opportunity>> DeleteOpportunity(string id)
        {
            var opportunity = await _context.Opportunities.Include(o => o.Owner).Include(o => o.Product).Include(o => o.Lead).FirstOrDefaultAsync(o => o.Id == id);
            if (opportunity == null)
            {
                return NotFound();
            }

            _context.Opportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            return opportunity;
        }

        private bool OpportunityExists(string id)
        {
            return _context.Opportunities.Any(e => e.Id == id);
        }
    }
}
