using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Domain.Common;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.DeactivateAccount;

public sealed class DeactivateAccountCommandHandler
    : ICommandHandler<DeactivateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateAccountCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasherService passwordHasherService,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _passwordHasherService = passwordHasherService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        DeactivateAccountCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.AccountId is null || _currentUserService.AccountId == Guid.Empty)
            return Result.Failure(AccountErrors.Unauthorized);

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure(AccountErrors.InvalidPassword);

        var account = await _accountRepository.GetByIdAsync(_currentUserService.AccountId.Value, cancellationToken);

        if (account is null)
            return Result.Failure(AccountErrors.InvalidAccount);

        if (!account.IsActive)
            return Result.Failure(AccountErrors.InactiveAccount);

        if (!_passwordHasherService.Verify(request.Password, account.PasswordHash))
            return Result.Failure(AccountErrors.InvalidPassword);

        account.Deactivate();

        await _accountRepository.UpdateAsync(account, cancellationToken);

        return Result.Success();
    }
}