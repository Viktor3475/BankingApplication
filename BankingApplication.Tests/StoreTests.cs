using BankingApplication.Data;
using BankingApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApplication.Tests;

public sealed class StoreTests
{
    [Fact]
    public async Task AccountQueriesNeverReturnAnotherUsersAccount()
    {
        await using var database = await TestDatabase.CreateAsync();
        using var scope = database.Services.CreateScope();
        var profiles = scope.ServiceProvider.GetRequiredService<BankUserStore>();
        var accounts = scope.ServiceProvider.GetRequiredService<AccountStore>();
        var first = NewRegistration("first@example.com", "BG80BNBG96611020345678");
        var second = NewRegistration("second@example.com", "GB29NWBK60161331926819");

        Assert.True((await profiles.RegisterAsync(first.User, first.Account, "ExamplePass123!", default)).Succeeded);
        Assert.True((await profiles.RegisterAsync(second.User, second.Account, "ExamplePass123!", default)).Succeeded);

        Assert.Null(await accounts.GetAsync(second.Account.Id, first.User.Id, default));
        var visible = await accounts.GetAllAsync(first.User.Id, default);
        Assert.Single(visible);
        Assert.Equal(first.Account.Id, visible[0].Id);
    }

    [Fact]
    public async Task FailedAccountInsertRollsBackNewIdentityAndProfile()
    {
        await using var database = await TestDatabase.CreateAsync();
        var first = NewRegistration("first@example.com", "BG80BNBG96611020345678");
        var duplicate = NewRegistration("second@example.com", first.Account.Iban);

        using (var scope = database.Services.CreateScope())
        {
            var profiles = scope.ServiceProvider.GetRequiredService<BankUserStore>();
            Assert.True((await profiles.RegisterAsync(first.User, first.Account, "ExamplePass123!", default)).Succeeded);
            await Assert.ThrowsAsync<DbUpdateException>(() =>
                profiles.RegisterAsync(duplicate.User, duplicate.Account, "ExamplePass123!", default));
        }

        using (var scope = database.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
            Assert.False(await db.Users.AnyAsync(user => user.Id == duplicate.User.Id));
            Assert.False(await db.BankUsers.AnyAsync(user => user.Id == duplicate.User.Id));
            Assert.False(await db.Accounts.AnyAsync(account => account.BankUserId == duplicate.User.Id));
        }
    }

    private static (BankUser User, Account Account) NewRegistration(string email, string iban)
    {
        var user = new BankUser { Username = email, Email = email, Country = Country.BG };
        var account = new Account
        {
            Iban = iban,
            BankUserId = user.Id,
            BankUser = user,
            AccountType = AccountType.Current
        };
        return (user, account);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public ServiceProvider Services { get; }

        private TestDatabase(SqliteConnection connection, ServiceProvider services)
        {
            _connection = connection;
            Services = services;
        }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<BankingDbContext>(options => options.UseSqlite(connection));
            services.AddIdentityCore<IdentityUser<Guid>>().AddEntityFrameworkStores<BankingDbContext>();
            services.AddScoped<BankUserStore>();
            services.AddScoped<AccountStore>();
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<BankingDbContext>().Database.EnsureCreatedAsync();
            return new TestDatabase(connection, provider);
        }

        public async ValueTask DisposeAsync()
        {
            await Services.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
