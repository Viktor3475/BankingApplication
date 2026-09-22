using BankingApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Data;

/// <summary>Maps bank users and accounts to PostgreSQL tables and enforces database constraints.</summary>
public sealed class BankingDbContext(DbContextOptions<BankingDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    /// <summary>Profiles linked one-to-one to Identity users by the same GUID.</summary>
    public DbSet<BankUser> BankUsers => Set<BankUser>();
    /// <summary>Accounts owned by profiles.</summary>
    public DbSet<Account> Accounts => Set<Account>();
    /// <summary>Immutable transfers between accounts.</summary>
    public DbSet<MoneyMovement> MoneyMovements => Set<MoneyMovement>();

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

        modelBuilder.Entity<MoneyMovement>(entity =>
        {
            entity.ToTable("money_movements");
            entity.HasKey(movement => movement.Id);
            entity.Property(movement => movement.Amount).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(movement => movement.Description).HasMaxLength(140);
            entity.Property(movement => movement.CreatedAtUtc).IsRequired();
            entity.HasIndex(movement => movement.CreatedAtUtc);
            entity.HasOne(movement => movement.SourceAccount).WithMany(account => account.SentMovements)
                .HasForeignKey(movement => movement.SourceAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(movement => movement.DestinationAccount).WithMany(account => account.ReceivedMovements)
                .HasForeignKey(movement => movement.DestinationAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("ck_money_movements_amount_positive", "\"Amount\" > 0");
                table.HasCheckConstraint("ck_money_movements_distinct_accounts",
                    "\"SourceAccountId\" <> \"DestinationAccountId\"");
            });
        });
    }
}
