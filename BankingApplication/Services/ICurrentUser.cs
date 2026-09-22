namespace BankingApplication.Services;

/// <summary>The authenticated profile ID associated with the current request.</summary>
public interface ICurrentUser
{
    /// <summary>Gets the profile ID from the validated authentication principal.</summary>
    Guid Id { get; }
}
