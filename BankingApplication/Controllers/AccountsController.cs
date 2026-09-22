using BankingApplication.Dtos;
using BankingApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers;

/// <summary>HTTP endpoints for reading accounts and requesting a new account.</summary>
[ApiController]
[Route("api/accounts")]
[Authorize]
public sealed class AccountsController(IAccountService accounts, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Gets one of the signed-in user's accounts; another owner's ID returns 404.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var account = await accounts.GetAsync(id, currentUser.Id, cancellationToken);
        return account is null ? NotFound() : Ok(account);
    }

    /// <summary>Lists accounts owned by the signed-in user.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await accounts.GetAllAsync(currentUser.Id, cancellationToken));
    }

    /// <summary>Opens a zero-balance account and returns its server-generated demo IBAN.</summary>
    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountDto request, CancellationToken cancellationToken)
    {
        var result = await accounts.CreateAsync(request, currentUser.Id, cancellationToken);
        if (result.Error is not null)
            return result.IsConflict ? Conflict(result.Error) : BadRequest(result.Error);

        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value);
    }
}
