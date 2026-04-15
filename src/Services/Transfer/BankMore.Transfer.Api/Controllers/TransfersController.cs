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

    /// <summary>
    /// Efetua transferência entre contas da mesma instituição.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return NoContent();
    }
}