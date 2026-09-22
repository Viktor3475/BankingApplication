using BankingApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Data;

/// <summary>Maps bank users and accounts to PostgreSQL tables and enforces database constraints.</summary>
public sealed class BankingDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    /// <summary>Creates a context that forces a transaction for each SaveChanges call.</summary>
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
    {
        // Each SaveChanges call gets a transaction, including writes that need only one SQL statement.
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
    }

    /// <summary>Profiles linked one-to-one to Identity users by the same GUID.</summary>
    public DbSet<BankUser> BankUsers => Set<BankUser>();
    /// <summary>Accounts owned by profiles.</summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>Defines schema rules that EF conventions cannot infer from the entity classes.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<IdentityUser<Guid>>().HasIndex(user => user.NormalizedEmail).IsUnique();
        modelBuilder.Entity<BankUser>(entity =>
        {
            entity.ToTable("bank_users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Username).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Country).HasConversion<string>().HasMaxLength(2).IsRequired();
            entity.HasMany(user => user.BankAccounts).WithOne(account => account.BankUser)
                .HasForeignKey(account => account.BankUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<IdentityUser<Guid>>().WithOne()
                .HasForeignKey<BankUser>(user => user.Id).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Iban).HasMaxLength(34).IsRequired();
            entity.HasIndex(account => account.Iban).IsUnique();
            entity.Property(account => account.Balance).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(account => account.AccountType).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.ToTable(table => table.HasCheckConstraint("ck_accounts_balance_nonnegative", "\"Balance\" >= 0"));
        });
    }
}
