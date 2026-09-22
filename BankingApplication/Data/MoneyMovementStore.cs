using System.Data;
using BankingApplication.Models;
using BankingApplication.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BankingApplication.Data;

/// <summary>EF Core persistence for atomic balance transfers and movement history.</summary>
public sealed class MoneyMovementStore(BankingDbContext db) : IMoneyMovementStore
{
    /// <inheritdoc />
    public async Task<TransferWriteOutcome> TransferAsync(
        Guid sourceAccountId, Guid ownerId, string recipientIban, decimal amount,
        string? description, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        try
        {
            var source = await db.Accounts.SingleOrDefaultAsync(
                account => account.Id == sourceAccountId && account.BankUserId == ownerId,
                cancellationToken);
            if (source is null) return new(TransferWriteResult.SourceNotFound);

            var destination = await db.Accounts.SingleOrDefaultAsync(
                account => account.Iban == recipientIban, cancellationToken);
            if (destination is null) return new(TransferWriteResult.DestinationNotFound);
            if (destination.Id == source.Id) return new(TransferWriteResult.SameAccount);
            if (source.Balance < amount) return new(TransferWriteResult.InsufficientFunds);

            source.Balance -= amount;
            destination.Balance += amount;
            var movement = new MoneyMovement
            {
                SourceAccountId = source.Id,
                DestinationAccountId = destination.Id,
                Amount = amount,
                Description = description,
                CreatedAtUtc = DateTime.UtcNow,
                SourceAccount = source,
                DestinationAccount = destination
            };
            db.MoneyMovements.Add(movement);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(TransferWriteResult.Succeeded, movement);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            return new(TransferWriteResult.ConcurrentUpdate);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.SerializationFailure })
        {
            return new(TransferWriteResult.ConcurrentUpdate);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MoneyMovement>> GetHistoryAsync(
        Guid ownerId, CancellationToken cancellationToken) =>
        await db.MoneyMovements.AsNoTracking()
            .Include(movement => movement.SourceAccount)
            .Include(movement => movement.DestinationAccount)
            .Where(movement => movement.SourceAccount.BankUserId == ownerId ||
                               movement.DestinationAccount.BankUserId == ownerId)
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
