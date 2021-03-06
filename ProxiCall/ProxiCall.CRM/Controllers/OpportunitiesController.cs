﻿using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Areas.Identity.Data;
using ProxiCall.CRM.Models;
using ProxiCall.Library.Enumeration.Opportunity;

namespace ProxiCall.CRM.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class OpportunitiesController : Controller
    {
        private readonly ProxicallCRMContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpportunitiesController(ProxicallCRMContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Opportunities
        public async Task<IActionResult> Index()
        {
            var proxicallCRMContext = _context.Opportunities.Include(o => o.Lead).Include(o => o.Owner).Include(o => o.Product);
            return View(await proxicallCRMContext.ToListAsync());
        }

        // GET: Opportunities/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities
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
            ViewData["LeadId"] = new SelectList(_context.Leads, "Id", "FullName");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title");
            ViewData["Confidence"] = new SelectList((IEnumerable)OpportunityConfidence.AllConfidenceDisplay, "Key", "Value");
            ViewData["Status"] = new SelectList((IEnumerable)OpportunityStatus.AllStatusDisplay, "Key", "Value");
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
            ViewData["LeadId"] = new SelectList(_context.Leads, "Id", "FullName", opportunity.LeadId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title", opportunity.ProductId);
            ViewData["Confidence"] = new SelectList((IEnumerable)OpportunityConfidence.AllConfidenceDisplay, "Key", "Value", opportunity.Confidence);
            ViewData["Status"] = new SelectList((IEnumerable)OpportunityStatus.AllStatusDisplay, "Key", "Value", opportunity.Status);
            return View(opportunity);
        }

        // GET: Opportunities/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity == null)
            {
                return NotFound();
            }
            ViewData["LeadId"] = new SelectList(_context.Leads, "Id", "FullName", opportunity.LeadId);
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", opportunity.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title", opportunity.ProductId);
            ViewData["Confidence"] = new SelectList(OpportunityConfidence.AllConfidenceDisplay, "Key", "Value", opportunity.Confidence);
            ViewData["Status"] = new SelectList(OpportunityStatus.AllStatusDisplay, "Key", "Value", opportunity.Status);
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
            ViewData["LeadId"] = new SelectList(_context.Leads, "Id", "FullName", opportunity.LeadId);
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", opportunity.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title", opportunity.ProductId);
            ViewData["Confidence"] = new SelectList(OpportunityConfidence.AllConfidenceDisplay, "Key", "Value", opportunity.Confidence);
            ViewData["Status"] = new SelectList(OpportunityStatus.AllStatusDisplay, "Key", "Value", opportunity.Status);
            return View(opportunity);
        }

        // GET: Opportunities/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities
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
            var opportunity = await _context.Opportunities.FindAsync(id);
            _context.Opportunities.Remove(opportunity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OpportunityExists(string id)
        {
            return _context.Opportunities.Any(e => e.Id == id);
        }
    }
}
