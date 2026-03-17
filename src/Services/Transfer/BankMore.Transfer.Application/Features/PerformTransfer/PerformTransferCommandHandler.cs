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
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountApiClient _accountApiClient;
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
        if (_currentUserService.AccountId is null || _currentUserService.AccountId == Guid.Empty)
            return Result<TransferResponse>.Failure(TransferErrors.Unauthorized);

        if (string.IsNullOrWhiteSpace(_currentUserService.BearerToken))
            return Result<TransferResponse>.Failure(TransferErrors.Unauthorized);

        if (string.IsNullOrWhiteSpace(request.RequestId))
            return Result<TransferResponse>.Failure(
                new Error("INVALID_REQUEST_ID", "O identificador da requisição é obrigatório."));

        if (string.IsNullOrWhiteSpace(request.DestinationAccountNumber))
            return Result<TransferResponse>.Failure(TransferErrors.InvalidAccount);

        if (request.Amount <= 0)
            return Result<TransferResponse>.Failure(TransferErrors.InvalidValue);

        if (await _idempotencyService.ExistsAsync(Scope, request.RequestId, cancellationToken))
        {
            var existing = await _transferRepository.GetByRequestIdAsync(request.RequestId, cancellationToken);

            if (existing is null)
            {
                return Result<TransferResponse>.Failure(
                    new Error("DUPLICATE_REQUEST_WITHOUT_RECORD", "A requisição já foi processada, mas não foi possível localizar a transferência."));
            }

            return Result<TransferResponse>.Success(new TransferResponse
            {
                TransferId = existing.Id,
                RequestId = existing.RequestId,
                SourceAccountId = existing.SourceAccountId,
                SourceAccountNumber = existing.SourceAccountNumber,
                DestinationAccountId = existing.DestinationAccountId,
                DestinationAccountNumber = existing.DestinationAccountNumber,
                Amount = existing.Amount,
                CreatedAtUtc = existing.CreatedAtUtc
            });
        }

        var bearerToken = _currentUserService.BearerToken!;

        var sourceAccount = await _accountApiClient.GetCurrentAccountAsync(
            bearerToken,
            cancellationToken);

        if (sourceAccount is null)
            return Result<TransferResponse>.Failure(TransferErrors.InvalidAccount);

        var destinationAccount = await _accountApiClient.GetAccountByNumberAsync(
            request.DestinationAccountNumber,
            bearerToken,
            cancellationToken);

        if (destinationAccount is null)
            return Result<TransferResponse>.Failure(TransferErrors.InvalidAccount);

        if (sourceAccount.AccountId == destinationAccount.AccountId)
        {
            return Result<TransferResponse>.Failure(
                new Error("INVALID_TRANSFER", "A conta de origem e a conta de destino não podem ser a mesma."));
        }

        var debitResult = await _accountApiClient.CreateDebitAsync(
            requestId: $"{request.RequestId}-debit",
            amount: request.Amount,
            bearerToken: bearerToken,
            cancellationToken: cancellationToken);

        if (!debitResult.IsSuccess)
        {
            return Result<TransferResponse>.Failure(new Error(
                debitResult.ErrorCode ?? "TRANSFER_DEBIT_FAILED",
                debitResult.ErrorMessage ?? "Falha ao debitar a conta de origem."));
        }

        var creditResult = await _accountApiClient.CreateCreditAsync(
            requestId: $"{request.RequestId}-credit",
            destinationAccountNumber: request.DestinationAccountNumber,
            amount: request.Amount,
            bearerToken: bearerToken,
            cancellationToken: cancellationToken);

        if (!creditResult.IsSuccess)
        {
            await _accountApiClient.RevertDebitWithCreditAsync(
                requestId: $"{request.RequestId}-rollback",
                amount: request.Amount,
                bearerToken: bearerToken,
                cancellationToken: cancellationToken);

            return Result<TransferResponse>.Failure(new Error(
                creditResult.ErrorCode ?? "TRANSFER_CREDIT_FAILED",
                creditResult.ErrorMessage ?? "Falha ao creditar a conta de destino."));
        }

        var transfer = TransferOperation.Create(
            requestId: request.RequestId,
            sourceAccountId: sourceAccount.AccountId,
            sourceAccountNumber: sourceAccount.AccountNumber,
            destinationAccountId: destinationAccount.AccountId,
            destinationAccountNumber: destinationAccount.AccountNumber,
            amount: request.Amount);

        await _transferRepository.AddAsync(transfer, cancellationToken);
        await _idempotencyService.RegisterAsync(Scope, request.RequestId, cancellationToken);

        await _transferEventPublisher.PublishCompletedAsync(
            transferId: transfer.Id,
            sourceAccountId: transfer.SourceAccountId,
            destinationAccountId: transfer.DestinationAccountId,
            amount: transfer.Amount,
            occurredOnUtc: transfer.CreatedAtUtc,
            cancellationToken: cancellationToken);

        return Result<TransferResponse>.Success(new TransferResponse
        {
            TransferId = transfer.Id,
            RequestId = transfer.RequestId,
            SourceAccountId = transfer.SourceAccountId,
            SourceAccountNumber = transfer.SourceAccountNumber,
            DestinationAccountId = transfer.DestinationAccountId,
            DestinationAccountNumber = transfer.DestinationAccountNumber,
            Amount = transfer.Amount,
            CreatedAtUtc = transfer.CreatedAtUtc
        });
    }
}