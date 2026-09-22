using BankingApplication.Dtos;
using BankingApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers;

/// <summary>HTTP endpoint for the authenticated user's profile.</summary>
[ApiController]
[Route("api/bank-users")]
public sealed class BankUsersController(IBankUserService users, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Gets the profile named by the validated bearer token.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<BankUserDto>> GetMe(CancellationToken cancellationToken)
    {
        var user = await users.GetAsync(currentUser.Id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
