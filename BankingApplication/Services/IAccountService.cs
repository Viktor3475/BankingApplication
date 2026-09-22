using BankingApplication.Dtos;

namespace BankingApplication.Services;

/// <summary>Account operations exposed to the HTTP layer; responses use DTOs rather than EF entities.</summary>
public interface IAccountService
{
    /// <summary>Returns an account only when it belongs to the authenticated owner.</summary>
    Task<AccountDto?> GetAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);
    /// <summary>Lists accounts belonging to the authenticated owner.</summary>
    Task<IReadOnlyList<AccountDto>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken);
    /// <summary>Opens a zero-balance account with a generated demo IBAN for the owner.</summary>
    Task<ServiceResult<AccountDto>> CreateAsync(CreateAccountDto request, Guid ownerId, CancellationToken cancellationToken);
}
