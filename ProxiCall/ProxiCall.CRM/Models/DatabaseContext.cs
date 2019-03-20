using Microsoft.EntityFrameworkCore;

namespace ProxiCall.CRM.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base (options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
