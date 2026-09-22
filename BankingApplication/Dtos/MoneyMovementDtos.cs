using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Dtos;

/// <summary>Fields accepted when sending money from an owned account.</summary>
public sealed record SendMoneyDto(
    [Required, StringLength(34, MinimumLength = 15)] string RecipientIban,
    [Range(typeof(decimal), "0.01", "9999999999999999.99")] decimal Amount,
    [StringLength(140)] string? Description);

/// <summary>How a movement affected one of the signed-in user's accounts.</summary>
public enum MoneyMovementDirection { Sent, Received }

/// <summary>A transfer shown from the signed-in user's perspective.</summary>
public sealed record MoneyMovementDto(
    Guid Id,
    MoneyMovementDirection Direction,
    Guid AccountId,
    string AccountIban,
    string CounterpartyIban,
    decimal Amount,
    string? Description,
    DateTime CreatedAtUtc);
