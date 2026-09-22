using BankingApplication.Dtos;
using BankingApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers;

/// <summary>Authenticated transfer and movement-history API endpoints.</summary>
[ApiController]
[Route("api")]
[Authorize]
public sealed class MoneyMovementsController(IMoneyMovementService movements, ICurrentUser currentUser)
    : ControllerBase
{
    /// <summary>Sends money from an account owned by the signed-in user.</summary>
    [HttpPost("accounts/{sourceAccountId:guid}/transfers")]
    public async Task<ActionResult<MoneyMovementDto>> Send(
        Guid sourceAccountId, SendMoneyDto request, CancellationToken cancellationToken)
    {
        var result = await movements.SendAsync(
            sourceAccountId, currentUser.Id, request, cancellationToken);
        return result.Error is null ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Lists sent and received movements for all accounts owned by the signed-in user.</summary>
    [HttpGet("money-movements")]
    public async Task<ActionResult<IReadOnlyList<MoneyMovementDto>>> GetHistory(
        CancellationToken cancellationToken) =>
        Ok(await movements.GetHistoryAsync(currentUser.Id, cancellationToken));
}
