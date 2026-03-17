using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.DTOs;
using BankMore.Account.Domain.Common;
using BankMore.Account.Domain.ValueObjects;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.Login;

public sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ITokenProvider _tokenProvider;

    public LoginCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasherService passwordHasherService,
        ITokenProvider tokenProvider)
    {
        _accountRepository = accountRepository;
        _passwordHasherService = passwordHasherService;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return Result<LoginResponse>.Failure(AccountErrors.Unauthorized);

        var account = await ResolveAccountAsync(request.Login, cancellationToken);

        if (account is null)
            return Result<LoginResponse>.Failure(AccountErrors.Unauthorized);

        if (!_passwordHasherService.Verify(request.Password, account.PasswordHash))
            return Result<LoginResponse>.Failure(AccountErrors.Unauthorized);

        if (!account.IsActive)
            return Result<LoginResponse>.Failure(AccountErrors.InactiveAccount);

        var token = _tokenProvider.GenerateToken(account.Id, account.AccountNumber, account.Name);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            Name = account.Name,
            Token = token
        });
    }

    private async Task<Domain.Entities.CurrentAccount?> ResolveAccountAsync(
        string login,
        CancellationToken cancellationToken)
    {
        var normalized = Cpf.Normalize(login);

        if (normalized.Length == 11)
        {
            return await _accountRepository.GetByCpfAsync(normalized, cancellationToken);
        }

        return await _accountRepository.GetByAccountNumberAsync(login.Trim(), cancellationToken);
    }
}