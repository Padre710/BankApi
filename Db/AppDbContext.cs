using BankApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionType)
                .HasConversion<string>(); // Store enum as string instead of int

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionDirection)
                .HasConversion<string>();

            modelBuilder.Entity<Account>()
                .Property(t => t.AccountType)
                .HasConversion<string>();

            modelBuilder.Entity<Account>()
               .Property(t => t.Currency)
               .HasConversion<string>();


            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Transaction>()
        .HasOne(t => t.Account)                
        .WithMany(a => a.Transactions)         
        .HasForeignKey(t => t.AccountId)
        .OnDelete(DeleteBehavior.Cascade);
            

            base.OnModelCreating(modelBuilder);
        }

    }
}
