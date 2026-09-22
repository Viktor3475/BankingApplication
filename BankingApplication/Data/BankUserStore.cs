using BankingApplication.Models;
using BankingApplication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BankingApplication.Data;

/// <summary>Persists Identity credentials, profile, and starting account as one registration.</summary>
public sealed class BankUserStore(BankingDbContext db,
    UserManager<IdentityUser<Guid>> identityUsers) : IBankUserStore
{
    /// <inheritdoc />
    public Task<BankUser?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.BankUsers.AsNoTracking().SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<RegistrationWriteResult> RegisterAsync(
        BankUser user, Account account, string password, CancellationToken cancellationToken)
    {
        var identityUser = new IdentityUser<Guid>
        {
            Id = user.Id,
            UserName = user.Email,
            Email = user.Email
        };

        try
        {
            // Identity saves credentials before the profile. The outer transaction covers both saves.
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var identityResult = await identityUsers.CreateAsync(identityUser, password);
            if (!identityResult.Succeeded)
                return new RegistrationWriteResult(false,
                    string.Join(" ", identityResult.Errors.Select(error => error.Description)),
                    identityResult.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"));

            db.BankUsers.Add(user);
            db.Accounts.Add(account);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new RegistrationWriteResult(true);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return new RegistrationWriteResult(false, "Email or IBAN already exists.", true);
        }
    }
}
