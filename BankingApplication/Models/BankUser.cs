namespace BankingApplication.Models;

/// <summary>Countries accepted by the profile model; US cannot receive an IBAN in this prototype.</summary>
public enum Country { UK, US, BG, RO, RU, TR }

/// <summary>Persisted profile and owner of zero or more accounts.</summary>
public sealed class BankUser
{
    /// <summary>Matches the ASP.NET Core Identity user ID for this profile.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Display name supplied at registration.</summary>
    public required string Username { get; set; }
    /// <summary>Normalized registration email; unique in the database.</summary>
    public required string Email { get; set; }
    /// <summary>Determines the format used for generated demo IBANs.</summary>
    public Country Country { get; set; }
    /// <summary>EF navigation to accounts owned by this profile.</summary>
    public List<Account> BankAccounts { get; set; } = [];
}
