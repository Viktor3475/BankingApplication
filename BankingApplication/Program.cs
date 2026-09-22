
using BankingApplication.Data;
using BankingApplication.Auth;
using BankingApplication.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication;

/// <summary>Configures the API, authentication, persistence, and startup migration.</summary>
public class Program
{
    /// <summary>Starts the ASP.NET Core application.</summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddOpenApi();
        var connectionString = builder.Configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("ConnectionStrings:Banking is required.");
        builder.Services.AddDbContext<BankingDbContext>(options => options.UseNpgsql(connectionString));
        // Identity stores password hashes in the same database as the banking profile.
        builder.Services.AddIdentityCore<IdentityUser<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddEntityFrameworkStores<BankingDbContext>().AddSignInManager();
        builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);
        // New endpoints require authentication unless explicitly marked AllowAnonymous.
        builder.Services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build());
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IBankUserService, BankUserService>();
        builder.Services.AddScoped<IAuthService, IdentityAuthService>();
        builder.Services.AddScoped<IAccountStore, AccountStore>();
        builder.Services.AddScoped<IBankUserStore, BankUserStore>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
        builder.Services.AddSingleton<DemoIbanGenerator>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            // Apply tracked schema changes before accepting requests.
            var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
            await db.Database.MigrateAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
