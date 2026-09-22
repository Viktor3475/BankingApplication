using BankingApplication.Dtos;

namespace BankingApplication.Services;

/// <summary>Bank user operations, including creation of the initial Current account.</summary>
public interface IBankUserService
{
    /// <summary>Returns a profile by ID; the HTTP layer supplies only the authenticated ID.</summary>
    Task<BankUserDto?> GetAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Registers a credential, profile, and initial Current account together.</summary>
    Task<ServiceResult<CreatedBankUserDto>> CreateAsync(RegisterUserDto request, CancellationToken cancellationToken);
}
