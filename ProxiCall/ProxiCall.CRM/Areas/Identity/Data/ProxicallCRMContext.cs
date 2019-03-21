using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProxiCall.CRM.Models;

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

        public DbSet<ProxiCall.CRM.Models.Company> Company { get; set; }
    }
}
