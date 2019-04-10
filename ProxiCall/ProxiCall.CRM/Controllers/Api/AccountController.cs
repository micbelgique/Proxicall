using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProxiCall.CRM.Areas.Identity.Data;
using ProxiCall.CRM.Models;

namespace ProxiCall.CRM.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ProxicallCRMContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(ProxicallCRMContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<ActionResult<LoginDTO>> Login(string phonenumber)
        {
            if(string.IsNullOrEmpty(phonenumber))
            {
                return BadRequest();
            }

            phonenumber = phonenumber.Trim();

            var user = _context.Set<IdentityUser>().FirstOrDefault(u => u.PhoneNumber == phonenumber);
                       
            if (user == null)
            {
                return NotFound(new { message = "No user with this phone number has been found" });
            }

            await _signInManager.SignInAsync(user, true);

            var response = new LoginDTO();
            response.Id = user.Id;
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.PhoneNumber = user.PhoneNumber;
            response.Roles = await _userManager.GetRolesAsync(user);
            response.Token = await GenerateJwtToken(phonenumber, user);

            return response;
        }

        private async Task<string> GenerateJwtToken(string phonenumber, IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, phonenumber),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, roles[0])
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings")["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(60);

            var token = new JwtSecurityToken(
                _configuration.GetSection("AppSettings")["JwtIssuer"],
                _configuration.GetSection("AppSettings")["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}