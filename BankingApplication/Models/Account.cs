namespace BankingApplication.Models;

/// <summary>The two account products supported by the prototype.</summary>
public enum AccountType { Current, Savings }

/// <summary>Persisted account. Its IBAN and initial balance are assigned by the server.</summary>
public sealed class Account
{
    /// <summary>Stable account ID used in API routes and foreign keys.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Server-generated demo IBAN; unique in the database.</summary>
    public required string Iban { get; set; }
    /// <summary>Stored decimal amount; writes are not exposed by this prototype.</summary>
    public decimal Balance { get; set; }
    /// <summary>Current or Savings product selected at account creation.</summary>
    public AccountType AccountType { get; set; }
    /// <summary>Foreign key to the owning profile.</summary>
    public Guid BankUserId { get; set; }
    /// <summary>EF navigation to the owning profile.</summary>
    public BankUser BankUser { get; set; } = null!;
    /// <summary>Transfers sent from this account.</summary>
    public List<MoneyMovement> SentMovements { get; set; } = [];
    /// <summary>Transfers received by this account.</summary>
    public List<MoneyMovement> ReceivedMovements { get; set; } = [];
}
