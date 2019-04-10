using Microsoft.AspNetCore.Hosting;
using ProxiCall.CRM.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace ProxiCall.CRM.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}