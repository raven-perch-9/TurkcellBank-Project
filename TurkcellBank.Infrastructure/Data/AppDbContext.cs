using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkcellBank.Domain;

namespace TurkcellBank.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Add your tables (DbSets) here
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CreditApplication> CreditApplications { get; set; }
        public DbSet<CreditInstallment> CreditInstallments { get; set; }

        // Adding Transaction Configuration Class to the Context
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        }
    }

    // Transaction Configuration Class - CAN BE SEPERATED LATER ON INTO A NEW FILE
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> e)
        {
            e.HasKey(t => t.ID);
            e.Property(t => t.Amount).HasPrecision(18, 2);
            e.Property(t => t.ReferenceCode).HasMaxLength(64);

            e.HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountID)
                .OnDelete(DeleteBehavior.NoAction);
            
            e.HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}