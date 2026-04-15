using BankMore.Account.Api.Contracts.Requests;
using BankMore.Account.Application.Features.CreateAccount;
using BankMore.Account.Application.Features.DeactivateAccount;
using BankMore.Account.Application.Features.GetCurrentAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Account.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Cadastra uma nova conta corrente.
    /// </summary>
    /// <remarks>
    /// Valida o CPF informado e retorna os dados da conta criada.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(
            request.Name,
            request.Cpf,
            request.Password);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Value.AccountId },
            result.Value);
    }

    /// <summary>
    /// Inativa a conta corrente autenticada.
    /// </summary>
    [HttpPatch("deactivate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(
        [FromBody] DeactivateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateAccountCommand(request.Password);

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

    /// <summary>
    /// Retorna os dados da conta corrente autenticada.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCurrentAccountQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                type = result.Error.Code,
                message = result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}