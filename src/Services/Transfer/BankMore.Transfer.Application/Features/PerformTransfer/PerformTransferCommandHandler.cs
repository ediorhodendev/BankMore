using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;
using BankMore.Transfer.Application.Abstractions.Persistence;
using BankMore.Transfer.Application.Abstractions.Security;
using BankMore.Transfer.Application.Abstractions.Services;
using BankMore.Transfer.Application.DTOs;
using BankMore.Transfer.Domain.Common;
using BankMore.Transfer.Domain.Entities;

namespace BankMore.Transfer.Application.Features.PerformTransfer;

public sealed class PerformTransferCommandHandler
    : ICommandHandler<PerformTransferCommand, TransferResponse>
{
    private const string Scope = "transfer";

    private readonly ITransferRepository _transferRepository;
    private readonly ITransferIdempotencyService _idempotencyService;
    private readonly IAccountApiClient _accountApiClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITransferEventPublisher _transferEventPublisher;

    public PerformTransferCommandHandler(
        ITransferRepository transferRepository,
        ITransferIdempotencyService idempotencyService,
        ICurrentUserService currentUserService,
        IAccountApiClient accountApiClient,
        ITransferEventPublisher transferEventPublisher)
    {
        _transferRepository = transferRepository;
        _idempotencyService = idempotencyService;
        _currentUserService = currentUserService;
        _accountApiClient = accountApiClient;
        _transferEventPublisher = transferEventPublisher;
    }

    public async Task<Result<TransferResponse>> Handle(
        PerformTransferCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.AccountId == Guid.Empty)
        {
            return Result<TransferResponse>.Failure(
                new Error("USER_UNAUTHORIZED", "Usuário não autenticado."));
        }

        if (string.IsNullOrWhiteSpace(_currentUserService.BearerToken))
        {
            return Result<TransferResponse>.Failure(
                new Error("USER_UNAUTHORIZED", "Token de acesso não informado."));
        }

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return Result<TransferResponse>.Failure(
                new Error("INVALID_REQUEST", "O identificador da requisição é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(request.DestinationAccountNumber))
        {
            return Result<TransferResponse>.Failure(TransferErrors.InvalidAccount);
        }

        if (request.Amount <= 0)
        {
            return Result<TransferResponse>.Failure(TransferErrors.InvalidValue);
        }

        var alreadyProcessed = await _idempotencyService.ExistsAsync(
            Scope,
            request.RequestId,
            cancellationToken);

        if (alreadyProcessed)
        {
            var existing = await _transferRepository.GetByRequestIdAsync(
                request.RequestId,
                cancellationToken);

            if (existing is not null)
            {
                return Result<TransferResponse>.Success(new TransferResponse
                {
                    TransferId = existing.Id,
                    RequestId = existing.RequestId,
                    SourceAccountId = existing.SourceAccountId,
                    DestinationAccountId = existing.DestinationAccountId,
                    Amount = existing.Amount,
                    CreatedAtUtc = existing.CreatedAtUtc
                });
            }

            return Result<TransferResponse>.Failure(
                new Error(
                    "IDEMPOTENCY_INCONSISTENCY",
                    "A requisição já foi processada, mas a transferência não foi encontrada."));
        }

        var bearerToken = _currentUserService.BearerToken!;

        var sourceAccount = await _accountApiClient.GetCurrentAccountAsync(
            bearerToken,
            cancellationToken);

        if (sourceAccount is null)
        {
            return Result<TransferResponse>.Failure(TransferErrors.InvalidAccount);
        }

        if (!sourceAccount.IsActive)
        {
            return Result<TransferResponse>.Failure(TransferErrors.InactiveAccount);
        }

        var destinationAccount = await _accountApiClient.ResolveAccountByNumberAsync(
            request.DestinationAccountNumber,
            bearerToken,
            cancellationToken);

        if (destinationAccount is null)
        {
            return Result<TransferResponse>.Failure(TransferErrors.InvalidAccount);
        }

        if (!destinationAccount.IsActive)
        {
            return Result<TransferResponse>.Failure(TransferErrors.InactiveAccount);
        }

        if (sourceAccount.AccountId == destinationAccount.AccountId)
        {
            return Result<TransferResponse>.Failure(
                new Error("INVALID_TRANSFER", "A conta de origem e a conta de destino não podem ser a mesma."));
        }

        var debitResult = await _accountApiClient.CreateDebitAsync(
            $"{request.RequestId}-debit",
            request.Amount,
            bearerToken,
            cancellationToken);

        if (!debitResult.IsSuccess)
        {
            return Result<TransferResponse>.Failure(new Error(
                debitResult.ErrorCode ?? "TRANSFER_DEBIT_FAILED",
                debitResult.ErrorMessage ?? "Falha ao debitar a conta de origem."));
        }

        var creditResult = await _accountApiClient.CreateCreditByAccountIdAsync(
            $"{request.RequestId}-credit",
            destinationAccount.AccountId,
            request.Amount,
            bearerToken,
            cancellationToken);

        if (!creditResult.IsSuccess)
        {
            await _accountApiClient.RevertDebitWithCreditAsync(
                $"{request.RequestId}-rollback",
                request.Amount,
                bearerToken,
                cancellationToken);

            return Result<TransferResponse>.Failure(new Error(
                creditResult.ErrorCode ?? "TRANSFER_CREDIT_FAILED",
                creditResult.ErrorMessage ?? "Falha ao creditar a conta de destino."));
        }

        var transfer = TransferOperation.Create(
            requestId: request.RequestId,
            sourceAccountId: sourceAccount.AccountId,
            destinationAccountId: destinationAccount.AccountId,
            amount: request.Amount);

        await _transferRepository.AddAsync(transfer, cancellationToken);

        await _idempotencyService.RegisterAsync(
            Scope,
            request.RequestId,
            cancellationToken);

        await _transferEventPublisher.PublishCompletedAsync(
            transfer.Id,
            transfer.SourceAccountId,
            transfer.DestinationAccountId,
            transfer.Amount,
            transfer.CreatedAtUtc,
            cancellationToken);

        return Result<TransferResponse>.Success(new TransferResponse
        {
            TransferId = transfer.Id,
            RequestId = transfer.RequestId,
            SourceAccountId = transfer.SourceAccountId,
            DestinationAccountId = transfer.DestinationAccountId,
            Amount = transfer.Amount,
            CreatedAtUtc = transfer.CreatedAtUtc
        });
    }
}