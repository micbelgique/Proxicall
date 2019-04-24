using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ProxiCall.CRM.Areas.Identity.Data;
using ProxiCall.CRM.Models;

namespace ProxiCall.CRM.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UsersController : Controller
    {
        private readonly ProxicallCRMContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsersController(ProxicallCRMContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View(_context.Set<ApplicationUser>());
        }
        
        public async Task<IActionResult> Call(string id)
        {
            var user = _context.Set<ApplicationUser>().FirstOrDefault(accountUser => accountUser.Id == id);
            using (var httpClient = new HttpClient())
            {
                //Todo manage potential error (number not found, no response,...)
                var path = $"http://proxicall.azurewebsites.net/api/voice/outbound/{user.PhoneNumber}";
                var response = await httpClient.GetAsync(path);
            }
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("UserName,Email,Password,ConfirmPassword")] NewUser userForm)
        {
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = userForm.Email,
                    Email = userForm.Email, 
                    Alias = userForm.Alias
                };
                var password = GenerateRandomPassword(_userManager.Options.Password);
                var result = await _userManager.CreateAsync(user, password.ToString());
                if (result.Succeeded)
                {
                    var role = userForm.IsAdmin ? "Admin" : "User";
                    await _userManager.AddToRoleAsync(user, role);
                    await SendEmailConfirmation(user);
                    //TODO add view for new user details -> username, email, role, isConfirmed
                    return View("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }

        private async Task SendEmailConfirmation(ApplicationUser user)
        {
            // For more information on how to enable account confirmation and password reset please 
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = new Uri($"{Request.Scheme}://{Request.Host}/Identity/Account/ResetPassword?code={Uri.EscapeDataString(code)}").ToString();
            await _emailSender.SendEmailAsync(
                user.Email,
                "Welcome",
                $"Welcome on Proxicall CRM</br>" +
                $"<i>One CRM to rule them all, One CRM to find them, One CRM to bring them all and in the darkness bind them.</i></br>" +
                $"Please choose your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        public string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        
        // GET: Users/Delete/5
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = _context.Set<ApplicationUser>()
                .FirstOrDefault(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Leads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Set<ApplicationUser>().FindAsync(id);
            _context.Set<ApplicationUser>().Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}