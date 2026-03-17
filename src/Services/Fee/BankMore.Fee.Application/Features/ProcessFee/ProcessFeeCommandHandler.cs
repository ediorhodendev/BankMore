using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;
using BankMore.Fee.Application.Abstractions.Persistence;
using BankMore.Fee.Application.Abstractions.Services;
using BankMore.Fee.Application.DTOs;
using BankMore.Fee.Domain.Common;
using BankMore.Fee.Domain.Entities;

namespace BankMore.Fee.Application.Features.ProcessFee;

public sealed class ProcessFeeCommandHandler
    : ICommandHandler<ProcessFeeCommand, FeeResponse>
{
    private readonly IFeeRepository _feeRepository;
    private readonly IFeeEventPublisher _feeEventPublisher;

    public ProcessFeeCommandHandler(
        IFeeRepository feeRepository,
        IFeeEventPublisher feeEventPublisher)
    {
        _feeRepository = feeRepository;
        _feeEventPublisher = feeEventPublisher;
    }

    public async Task<Result<FeeResponse>> Handle(
        ProcessFeeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TransferId == Guid.Empty)
            return Result<FeeResponse>.Failure(FeeErrors.InvalidTransfer);

        if (request.AccountId == Guid.Empty)
            return Result<FeeResponse>.Failure(FeeErrors.InvalidAccount);

        if (request.Amount <= 0)
            return Result<FeeResponse>.Failure(FeeErrors.InvalidValue);

        var existing = await _feeRepository.GetByTransferIdAsync(request.TransferId, cancellationToken);
        if (existing is not null)
        {
            return Result<FeeResponse>.Success(new FeeResponse
            {
                FeeId = existing.Id,
                TransferId = existing.TransferId,
                AccountId = existing.AccountId,
                Amount = existing.Amount,
                CreatedAtUtc = existing.CreatedAtUtc
            });
        }

        var fee = FeeOperation.Create(
            transferId: request.TransferId,
            accountId: request.AccountId,
            amount: request.Amount);

        await _feeRepository.AddAsync(fee, cancellationToken);

        await _feeEventPublisher.PublishCompletedAsync(
            feeId: fee.Id,
            accountId: fee.AccountId,
            transferId: fee.TransferId,
            amount: fee.Amount,
            occurredOnUtc: fee.CreatedAtUtc,
            cancellationToken: cancellationToken);

        return Result<FeeResponse>.Success(new FeeResponse
        {
            FeeId = fee.Id,
            TransferId = fee.TransferId,
            AccountId = fee.AccountId,
            Amount = fee.Amount,
            CreatedAtUtc = fee.CreatedAtUtc
        });
    }
}