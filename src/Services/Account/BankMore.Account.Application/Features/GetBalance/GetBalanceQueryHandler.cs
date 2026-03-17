using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.DTOs;
using BankMore.Account.Domain.Common;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.GetBalance;

public sealed class GetBalanceQueryHandler
    : IQueryHandler<GetBalanceQuery, BalanceResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBalanceQueryHandler(
        IAccountRepository accountRepository,
        IMovementRepository movementRepository,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BalanceResponse>> Handle(
        GetBalanceQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.AccountId is null || _currentUserService.AccountId == Guid.Empty)
            return Result<BalanceResponse>.Failure(AccountErrors.Unauthorized);

        var account = await _accountRepository.GetByIdAsync(
            _currentUserService.AccountId.Value,
            cancellationToken);

        if (account is null)
            return Result<BalanceResponse>.Failure(AccountErrors.InvalidAccount);

        if (!account.IsActive)
            return Result<BalanceResponse>.Failure(AccountErrors.InactiveAccount);

        var balance = await _movementRepository.GetBalanceAsync(account.Id, cancellationToken);

        return Result<BalanceResponse>.Success(new BalanceResponse
        {
            AccountNumber = account.AccountNumber,
            Name = account.Name,
            QueriedAtUtc = DateTime.UtcNow,
            Balance = balance
        });
    }
}