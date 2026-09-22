using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>Persistence operations needed by account rules.</summary>
public interface IAccountStore
{
    /// <summary>Finds an account scoped to its owner; other users' accounts appear absent.</summary>
    Task<Account?> GetAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);
    /// <summary>Lists only the owner's accounts in IBAN order.</summary>
    Task<IReadOnlyList<Account>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken);
    /// <summary>Returns the owner's country, or null when no profile exists.</summary>
    Task<Country?> GetOwnerCountryAsync(Guid ownerId, CancellationToken cancellationToken);
    /// <summary>Persists an account; returns false if a unique database constraint is violated.</summary>
    Task<bool> TryCreateAsync(Account account, CancellationToken cancellationToken);
}
