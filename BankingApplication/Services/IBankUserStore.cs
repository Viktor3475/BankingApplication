using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>Persistence operations needed by profile registration.</summary>
public interface IBankUserStore
{
    /// <summary>Finds a profile without tracking it for writes.</summary>
    Task<BankUser?> GetAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Persists credentials, profile, and account in one database transaction.</summary>
    Task<RegistrationWriteResult> RegisterAsync(
        BankUser user, Account account, string password, CancellationToken cancellationToken);
}

/// <summary>Outcome of the atomic registration write; conflicts represent uniqueness failures.</summary>
public sealed record RegistrationWriteResult(bool Succeeded, string? Error = null, bool IsConflict = false);
