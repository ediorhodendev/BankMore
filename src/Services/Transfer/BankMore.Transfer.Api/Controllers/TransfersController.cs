using BankMore.Transfer.Api.Contracts.Requests;
using BankMore.Transfer.Application.Features.PerformTransfer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Transfer.Api.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize]
public sealed class TransfersController : ControllerBase
{
    private readonly ISender _sender;

    public TransfersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Perform(
        [FromBody] PerformTransferRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PerformTransferCommand(
            request.RequestId,
            request.DestinationAccountNumber,
            request.Amount);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "USER_UNAUTHORIZED")
            {
                return Unauthorized(new
                {
                    type = result.Error.Code,
                    message = result.Error.Message
                });
            }

            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}