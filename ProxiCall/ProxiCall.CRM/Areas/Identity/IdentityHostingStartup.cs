using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proxicall.CRM.Models;

[assembly: HostingStartup(typeof(Proxicall.CRM.Areas.Identity.IdentityHostingStartup))]
namespace Proxicall.CRM.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<ProxicallCRMContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("ProxicallCRMContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<ProxicallCRMContext>();
            });
        }
    }
}