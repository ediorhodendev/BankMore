using BankMore.Account.Application.Features.GetAccountByNumber;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Account.Api.Controllers;

[ApiController]
[Route("api/internal/accounts")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class InternalAccountsController : ControllerBase
{
    private readonly ISender _sender;

    public InternalAccountsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("resolve/{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveByNumber(
        [FromRoute] string accountNumber,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAccountByNumberQuery(accountNumber), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(new
        {
            accountId = result.Value.AccountId,
            name = result.Value.Name,
            isActive = result.Value.IsActive
        });
    }
}