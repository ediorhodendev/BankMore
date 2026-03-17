using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.DTOs;
using BankMore.Account.Domain.Common;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.GetAccountByNumber;

public sealed class GetAccountByNumberQueryHandler
    : IQueryHandler<GetAccountByNumberQuery, AccountSummaryResponse>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByNumberQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<AccountSummaryResponse>> Handle(
        GetAccountByNumberQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            return Result<AccountSummaryResponse>.Failure(AccountErrors.InvalidAccount);

        var account = await _accountRepository.GetByAccountNumberAsync(
            request.AccountNumber.Trim(),
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