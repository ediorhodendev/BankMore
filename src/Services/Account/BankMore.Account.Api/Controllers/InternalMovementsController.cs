using BankMore.Account.Api.Contracts.Requests;
using BankMore.Account.Application.Features.CreateMovement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Account.Api.Controllers;

[ApiController]
[Route("api/internal/movements")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class InternalMovementsController : ControllerBase
{
    private readonly ISender _sender;

    public InternalMovementsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("credit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Credit(
        [FromBody] InternalCreateMovementRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMovementCommand(
            request.RequestId,
            null,
            request.TargetAccountId,
            request.Amount,
            request.Type);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "USER_UNAUTHORIZED")
                return Unauthorized(new { type = result.Error.Code, message = result.Error.Message });

            return BadRequest(new { type = result.Error.Code, message = result.Error.Message });
        }

        return NoContent();
    }
}