using BankingApplication.Dtos;
using BankingApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers;

/// <summary>Registers users and issues Identity bearer tokens at sign-in.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, IBankUserService users) : ControllerBase
{
    /// <summary>Creates credentials, a profile, and a starting account; returns 201 on success.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<CreatedBankUserDto>> Register(
        RegisterUserDto request, CancellationToken cancellationToken)
    {
        var result = await users.CreateAsync(request, cancellationToken);
        if (result.Error is not null)
            return result.IsConflict ? Conflict(result.Error) : BadRequest(result.Error);

        return Created("/api/bank-users/me", result.Value);
    }

    /// <summary>Verifies the password and writes an Identity bearer token response.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var principal = await auth.AuthenticateAsync(request.Email, request.Password);
        if (principal is null) return Unauthorized();
        await HttpContext.SignInAsync(IdentityConstants.BearerScheme, principal);
        return new EmptyResult();
    }
}
