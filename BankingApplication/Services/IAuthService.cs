using System.Security.Claims;

namespace BankingApplication.Services;

/// <summary>Verifies a password and returns a principal when sign-in succeeds.</summary>
public interface IAuthService
{
    /// <summary>Returns an Identity principal on successful password verification, otherwise null.</summary>
    Task<ClaimsPrincipal?> AuthenticateAsync(string email, string password);
}
