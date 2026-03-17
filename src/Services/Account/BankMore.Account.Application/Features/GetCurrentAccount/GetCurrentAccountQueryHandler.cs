using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.DTOs;
using BankMore.Account.Domain.Common;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.GetCurrentAccount;

public sealed class GetCurrentAccountQueryHandler
    : IQueryHandler<GetCurrentAccountQuery, AccountSummaryResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentAccountQueryHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AccountSummaryResponse>> Handle(
        GetCurrentAccountQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.AccountId is null || _currentUserService.AccountId == Guid.Empty)
            return Result<AccountSummaryResponse>.Failure(AccountErrors.Unauthorized);

        var account = await _accountRepository.GetByIdAsync(
            _currentUserService.AccountId.Value,
            cancellationToken);

        if (account is null)
            return Result<AccountSummaryResponse>.Failure(AccountErrors.InvalidAccount);

        return Result<AccountSummaryResponse>.Success(new AccountSummaryResponse
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            Name = account.Name,
            IsActive = account.IsActive
        });
    }
}