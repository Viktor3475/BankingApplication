using System.Security.Claims;
using BankingApplication.Services;
using Microsoft.AspNetCore.Identity;

namespace BankingApplication.Auth;

/// <summary>ASP.NET Core Identity adapter for password verification and lockout.</summary>
public sealed class IdentityAuthService(UserManager<IdentityUser<Guid>> userManager,
    SignInManager<IdentityUser<Guid>> signInManager) : IAuthService
{
    /// <inheritdoc />
    public async Task<ClaimsPrincipal?> AuthenticateAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null) return null;

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        return result.Succeeded ? await signInManager.CreateUserPrincipalAsync(user) : null;
    }
}
