using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>Atomic transfer and owner-scoped movement history persistence.</summary>
public interface IMoneyMovementStore
{
    /// <summary>Moves funds and records the transfer in one database transaction.</summary>
    Task<TransferWriteOutcome> TransferAsync(
        Guid sourceAccountId, Guid ownerId, string recipientIban, decimal amount,
        string? description, CancellationToken cancellationToken);
    /// <summary>Lists transfers involving any account owned by the user.</summary>
    Task<IReadOnlyList<MoneyMovement>> GetHistoryAsync(Guid ownerId, CancellationToken cancellationToken);
}

/// <summary>Possible outcomes from the atomic persistence operation.</summary>
public enum TransferWriteResult
{
    Succeeded,
    SourceNotFound,
    DestinationNotFound,
    SameAccount,
    InsufficientFunds,
    ConcurrentUpdate
}

/// <summary>Persistence result with the newly recorded movement on success.</summary>
public sealed record TransferWriteOutcome(TransferWriteResult Result, MoneyMovement? Movement = null);
