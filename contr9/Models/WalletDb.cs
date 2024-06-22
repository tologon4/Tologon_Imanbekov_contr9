using contr9.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace contr9.Models;

public class WalletDb : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceUser> ServiceUsers { get; set; }
    
    public WalletDb(DbContextOptions<WalletDb> options) : base(options){}
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.SenderUser)
            .HasForeignKey(t => t.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<Transaction>()
            .HasOne(t => t.RecipientUser)
            .WithMany()
            .HasForeignKey(t => t.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        new ServiceInitializer(builder).Seed();
    }
}