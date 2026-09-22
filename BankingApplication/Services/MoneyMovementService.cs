using BankingApplication.Dtos;
using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>Validates transfers and maps immutable movement records for clients.</summary>
public sealed class MoneyMovementService(IMoneyMovementStore store) : IMoneyMovementService
{
    /// <inheritdoc />
    public async Task<ServiceResult<MoneyMovementDto>> SendAsync(
        Guid sourceAccountId, Guid ownerId, SendMoneyDto request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0 || decimal.Round(request.Amount, 2) != request.Amount)
            return ServiceResult<MoneyMovementDto>.Invalid("Amount must be positive with at most two decimal places.");
        if (string.IsNullOrWhiteSpace(request.RecipientIban))
            return ServiceResult<MoneyMovementDto>.Invalid("Recipient IBAN is required.");

        var iban = request.RecipientIban.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var result = await store.TransferAsync(
            sourceAccountId, ownerId, iban, request.Amount, description, cancellationToken);
        if (result.Result != TransferWriteResult.Succeeded)
            return ServiceResult<MoneyMovementDto>.Invalid(result.Result switch
            {
                TransferWriteResult.SourceNotFound => "Source account does not exist.",
                TransferWriteResult.DestinationNotFound => "Recipient account does not exist.",
                TransferWriteResult.SameAccount => "Source and recipient accounts must be different.",
                TransferWriteResult.InsufficientFunds => "Insufficient funds.",
                TransferWriteResult.ConcurrentUpdate => "The balance changed during the transfer. Please try again.",
                _ => "Transfer failed."
            });

        return ServiceResult<MoneyMovementDto>.Success(ToDto(result.Movement!, ownerId));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MoneyMovementDto>> GetHistoryAsync(
        Guid ownerId, CancellationToken cancellationToken) =>
        (await store.GetHistoryAsync(ownerId, cancellationToken))
            .Select(movement => ToDto(movement, ownerId)).ToList();

    private static MoneyMovementDto ToDto(MoneyMovement movement, Guid ownerId)
    {
        var sent = movement.SourceAccount.BankUserId == ownerId;
        var account = sent ? movement.SourceAccount : movement.DestinationAccount;
        var counterparty = sent ? movement.DestinationAccount : movement.SourceAccount;
        return new MoneyMovementDto(movement.Id,
            sent ? MoneyMovementDirection.Sent : MoneyMovementDirection.Received,
            account.Id, account.Iban, counterparty.Iban, movement.Amount,
            movement.Description, movement.CreatedAtUtc);
    }
}
