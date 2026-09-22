using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankingApplication.Data;

/// <summary>Creates the context for EF migration commands without starting the web application.</summary>
public sealed class BankingDbContextFactory : IDesignTimeDbContextFactory<BankingDbContext>
{
    /// <summary>Uses the environment connection string when available; no connection is opened here.</summary>
    public BankingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Banking")
            ?? "Host=localhost;Database=banking_application;Username=postgres";
        var options = new DbContextOptionsBuilder<BankingDbContext>()
            .UseNpgsql(connectionString).Options;
        return new BankingDbContext(options);
    }
}
