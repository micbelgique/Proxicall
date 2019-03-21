using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Proxicall.CRM.Areas.Identity.Data
{
    public class RolesInitializer : IRolesInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration Configuration;

        public RolesInitializer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            Configuration = configuration;
        }

        public void Initialize()
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var role in roleNames)
            {
                var roleExist = roleManager.RoleExistsAsync(role).Result;
                if (!roleExist)
                {
                    roleResult = roleManager.CreateAsync(new IdentityRole(role)).Result;
                }
            }

            var admin = new IdentityUser
            {
                UserName = Configuration.GetSection("UserSettings")["UserName"],
                Email = Configuration.GetSection("UserSettings")["Email"]
            };

            var userPassword = Configuration.GetSection("UserSettings")["Password"];

            var _user = userManager.FindByEmailAsync(admin.Email).Result;
            if (_user == null)
            {
                var createAdmin = userManager.CreateAsync(admin, userPassword).Result;
                if (createAdmin.Succeeded)
                {
                    var roleAdded = userManager.AddToRolesAsync(admin, roleNames).Result;
                }
            }
        }

        public async Task InitializeAsync()
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var role in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var admin = new IdentityUser
            {
                UserName = Configuration.GetSection("UserSettings")["UserName"],
                Email = Configuration.GetSection("UserSettings")["Email"]
            };

            var userPassword = Configuration.GetSection("UserSettings")["Password"];

            var _user = await userManager.FindByEmailAsync(admin.Email);
            if (_user == null)
            {
                var createAdmin = await userManager.CreateAsync(admin, userPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, roleNames[0]);
                }
            }
        }
    }
}
