using BankingApplication.Models;
using BankingApplication.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BankingApplication.Data;

/// <summary>EF Core implementation of owner-filtered account persistence.</summary>
public sealed class AccountStore(BankingDbContext db) : IAccountStore
{
    /// <inheritdoc />
    public Task<Account?> GetAsync(Guid id, Guid ownerId, CancellationToken cancellationToken) =>
        db.Accounts.AsNoTracking().SingleOrDefaultAsync(
            account => account.Id == id && account.BankUserId == ownerId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Account>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken) =>
        await db.Accounts.AsNoTracking().Where(account => account.BankUserId == ownerId)
            .OrderBy(account => account.Iban).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<Country?> GetOwnerCountryAsync(Guid ownerId, CancellationToken cancellationToken) =>
        db.BankUsers.AsNoTracking().Where(user => user.Id == ownerId)
            .Select(user => (Country?)user.Country).SingleOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> TryCreateAsync(Account account, CancellationToken cancellationToken)
    {
        db.Accounts.Add(account);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return false;
        }
    }
}
