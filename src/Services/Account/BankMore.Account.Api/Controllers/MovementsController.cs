using BankMore.Account.Api.Contracts.Requests;
using BankMore.Account.Application.Features.CreateMovement;
using BankMore.Account.Application.Features.GetBalance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Account.Api.Controllers;

[ApiController]
[Route("api/movements")]
[Authorize]
public sealed class MovementsController : ControllerBase
{
    private readonly ISender _sender;

    public MovementsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMovementRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMovementCommand(
            request.RequestId,
            request.AccountNumber,
            request.Amount,
            request.Type);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "USER_UNAUTHORIZED")
                return Unauthorized(new { type = result.Error.Code, message = result.Error.Message });

            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return NoContent();
    }

    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance(CancellationToken cancellationToken)
    {
        var query = new GetBalanceQuery();

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "USER_UNAUTHORIZED")
                return Unauthorized(new { type = result.Error.Code, message = result.Error.Message });

            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}