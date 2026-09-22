namespace BankingApplication.Models;

/// <summary>An immutable transfer of money between two accounts.</summary>
public sealed class MoneyMovement
{
    /// <summary>Stable identifier for this transfer.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Account whose balance was reduced.</summary>
    public Guid SourceAccountId { get; set; }
    /// <summary>Account whose balance was increased.</summary>
    public Guid DestinationAccountId { get; set; }
    /// <summary>Positive amount transferred.</summary>
    public decimal Amount { get; set; }
    /// <summary>Optional sender-provided payment description.</summary>
    public string? Description { get; set; }
    /// <summary>UTC instant at which the transfer was recorded.</summary>
    public DateTime CreatedAtUtc { get; set; }
    /// <summary>Source account navigation.</summary>
    public Account SourceAccount { get; set; } = null!;
    /// <summary>Destination account navigation.</summary>
    public Account DestinationAccount { get; set; } = null!;
}
