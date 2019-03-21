using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Proxicall.CRM.Models;

namespace Proxicall.CRM.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsersController(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
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
                var user = new IdentityUser { UserName = userForm.Email, Email = userForm.Email };
                var result = await _userManager.CreateAsync(user, userForm.Password);
                if (result.Succeeded)
                {
                    var role = userForm.IsAdmin ? "Admin" : "User";
                    await _userManager.AddToRoleAsync(user, role);
                    //TODO send email to user
                    await _emailSender.SendEmailAsync(userForm.Email, "Finish creating your account", "Welcome to Proxicall CRM, the best CRM of all");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            //TODO add view for new user details -> username, email, role, isConfirmed
            return View();
        }
    }
}