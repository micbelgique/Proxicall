using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Models;

namespace ProxiCall.CRM.Areas.Identity.Data
{
    public class ProxicallCRMContext : IdentityDbContext<ApplicationUser>
    {
        public ProxicallCRMContext(DbContextOptions<ProxicallCRMContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Lead> Leads { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Opportunity> Opportunities { get; set; }
    }
}
