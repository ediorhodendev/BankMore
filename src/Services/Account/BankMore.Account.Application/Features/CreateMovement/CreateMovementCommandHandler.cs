using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Domain.Common;
using BankMore.Account.Domain.Entities;
using BankMore.Account.Domain.Enums;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.CreateMovement;

public sealed class CreateMovementCommandHandler : ICommandHandler<CreateMovementCommand>
{
    private const string Scope = "movement";

    private readonly IAccountRepository _accountRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly ICurrentUserService _currentUserService;

    public CreateMovementCommandHandler(
        IAccountRepository accountRepository,
        IMovementRepository movementRepository,
        IIdempotencyService idempotencyService,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _idempotencyService = idempotencyService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        CreateMovementCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.AccountId is null || _currentUserService.AccountId == Guid.Empty)
            return Result.Failure(AccountErrors.Unauthorized);

        if (string.IsNullOrWhiteSpace(request.RequestId))
            return Result.Failure(
                new Error("INVALID_REQUEST_ID", "O identificador da requisição é obrigatório."));

        if (request.Amount <= 0)
            return Result.Failure(AccountErrors.InvalidValue);

        if (await _idempotencyService.ExistsAsync(Scope, request.RequestId, cancellationToken))
            return Result.Success();

        var movementType = ParseMovementType(request.Type);
        if (movementType is null)
            return Result.Failure(AccountErrors.InvalidType);

        var currentAccount = await _accountRepository.GetByIdAsync(
            _currentUserService.AccountId.Value,
            cancellationToken);

        if (currentAccount is null)
            return Result.Failure(AccountErrors.InvalidAccount);

        if (!currentAccount.IsActive)
            return Result.Failure(AccountErrors.InactiveAccount);

        var targetAccount = currentAccount;

        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            targetAccount = await _accountRepository.GetByAccountNumberAsync(
                request.AccountNumber.Trim(),
                cancellationToken);

            if (targetAccount is null)
                return Result.Failure(AccountErrors.InvalidAccount);

            if (!targetAccount.IsActive)
                return Result.Failure(AccountErrors.InactiveAccount);

            var isDifferentAccount = targetAccount.Id != currentAccount.Id;

            if (isDifferentAccount && movementType != MovementType.Credit)
                return Result.Failure(AccountErrors.InvalidType);
        }

        var movement = Movement.Create(
            currentAccountId: targetAccount.Id,
            requestId: request.RequestId,
            type: movementType.Value,
            amount: request.Amount);

        await _movementRepository.AddAsync(movement, cancellationToken);
        await _idempotencyService.RegisterAsync(Scope, request.RequestId, cancellationToken);

        return Result.Success();
    }

    private static MovementType? ParseMovementType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return null;

        var normalized = type.Trim().ToUpperInvariant();

        return normalized switch
        {
            "C" => MovementType.Credit,
            "CREDIT" => MovementType.Credit,
            "D" => MovementType.Debit,
            "DEBIT" => MovementType.Debit,
            _ => null
        };
    }
}