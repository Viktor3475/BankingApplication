using System.Security.Claims;
using BankingApplication.Services;

namespace BankingApplication.Auth;

/// <summary>Reads the user ID claim from the authenticated HTTP request.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    /// <inheritdoc />
    public Guid Id
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("The authenticated user has no valid ID claim.");
        }
    }
}
