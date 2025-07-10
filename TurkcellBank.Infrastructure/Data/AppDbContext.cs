using Microsoft.EntityFrameworkCore;
using TurkcellBank.Domain;

namespace TurkcellBank.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Add your tables (DbSets) here
        public DbSet<User> Users { get; set; }

        // Later you might add more like:
        // public DbSet<Account> Accounts { get; set; }
        // public DbSet<Transaction> Transactions { get; set; }
    }
}