using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proxicall.CRM.Models;

namespace Proxicall.CRM.Models
{
    public class ProxicallCRMContext : IdentityDbContext<IdentityUser>
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
