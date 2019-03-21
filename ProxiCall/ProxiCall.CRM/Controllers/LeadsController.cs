using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Models;
using Proxicall.CRM.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProxiCall.CRM.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class LeadsController : Controller
    {
        private readonly ProxicallCRMContext _context;

        public LeadsController(ProxicallCRMContext context)
        {
            _context = context;
        }

        // GET: Leads
        public async Task<IActionResult> Index()
        {
            var proxicallCRMContext = _context.Lead.Include(l => l.Company);
            return View(await proxicallCRMContext.ToListAsync());
        }

        // GET: Leads/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Lead
                .Include(l => l.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lead == null)
            {
                return NotFound();
            }

            return View(lead);
        }

        // GET: Leads/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name");
            return View();
        }

        // POST: Leads/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,Email,Address,CompanyId")] Lead lead)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lead);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", lead.CompanyId);
            return View(lead);
        }

        // GET: Leads/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Lead.FindAsync(id);
            if (lead == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", lead.CompanyId);
            return View(lead);
        }

        // POST: Leads/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,PhoneNumber,Email,Address,CompanyId")] Lead lead)
        {
            if (id != lead.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lead);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeadExists(lead.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", lead.CompanyId);
            return View(lead);
        }

        // GET: Leads/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Lead
                .Include(l => l.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lead == null)
            {
                return NotFound();
            }

            return View(lead);
        }

        // POST: Leads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var lead = await _context.Lead.FindAsync(id);
            _context.Lead.Remove(lead);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeadExists(string id)
        {
            return _context.Lead.Any(e => e.Id == id);
        }
    }
}
