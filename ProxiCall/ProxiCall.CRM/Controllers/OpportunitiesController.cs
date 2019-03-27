using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;

namespace Proxicall.CRM.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class OpportunitiesController : Controller
    {
        private readonly ProxicallCRMContext _context;

        public OpportunitiesController(ProxicallCRMContext context)
        {
            _context = context;
        }

        // GET: Opportunities
        public async Task<IActionResult> Index()
        {
            var proxicallCRMContext = _context.Opportunity.Include(o => o.Lead).Include(o => o.Owner).Include(o => o.Product);
            return View(await proxicallCRMContext.ToListAsync());
        }

        // GET: Opportunities/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunity
                .Include(o => o.Lead)
                .Include(o => o.Owner)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        // GET: Opportunities/Create
        public IActionResult Create()
        {
            ViewData["LeadId"] = new SelectList(_context.Lead, "Id", "FullName");
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Title");
            return View();
        }

        // POST: Opportunities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OwnerId,LeadId,ProductId,CreationDate,EstimatedCloseDate,Comments,Status,Confidence")] Opportunity opportunity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(opportunity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeadId"] = new SelectList(_context.Lead, "Id", "FullName", opportunity.LeadId);
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", opportunity.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Title", opportunity.ProductId);
            return View(opportunity);
        }

        // GET: Opportunities/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunity.FindAsync(id);
            if (opportunity == null)
            {
                return NotFound();
            }
            ViewData["LeadId"] = new SelectList(_context.Lead, "Id", "FullName", opportunity.LeadId);
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", opportunity.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Title", opportunity.ProductId);
            return View(opportunity);
        }

        // POST: Opportunities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,OwnerId,LeadId,ProductId,CreationDate,EstimatedCloseDate,Comments,Status,Confidence")] Opportunity opportunity)
        {
            if (id != opportunity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(opportunity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpportunityExists(opportunity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeadId"] = new SelectList(_context.Lead, "Id", "FullName", opportunity.LeadId);
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", opportunity.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Title", opportunity.ProductId);
            return View(opportunity);
        }

        // GET: Opportunities/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunity
                .Include(o => o.Lead)
                .Include(o => o.Owner)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        // POST: Opportunities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var opportunity = await _context.Opportunity.FindAsync(id);
            _context.Opportunity.Remove(opportunity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OpportunityExists(string id)
        {
            return _context.Opportunity.Any(e => e.Id == id);
        }
    }
}
