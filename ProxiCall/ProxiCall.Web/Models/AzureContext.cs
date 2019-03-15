using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Web.Models
{
    public class AzureContext : DbContext
    {
        public AzureContext(DbContextOptions<AzureContext> options)
            :base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
