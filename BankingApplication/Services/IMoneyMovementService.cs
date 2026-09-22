using BankingApplication.Dtos;

namespace BankingApplication.Services;

/// <summary>Money transfer rules and movement history queries.</summary>
public interface IMoneyMovementService
{
    /// <summary>Sends money from an owned account to an account identified by IBAN.</summary>
    Task<ServiceResult<MoneyMovementDto>> SendAsync(
        Guid sourceAccountId, Guid ownerId, SendMoneyDto request, CancellationToken cancellationToken);
    /// <summary>Returns movements involving the signed-in user's accounts, newest first.</summary>
    Task<IReadOnlyList<MoneyMovementDto>> GetHistoryAsync(Guid ownerId, CancellationToken cancellationToken);
}
