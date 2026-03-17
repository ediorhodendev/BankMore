namespace BankMore.Fee.Application.Abstractions.Services;

public interface IFeeEventPublisher
{
    Task PublishCompletedAsync(
        Guid feeId,
        Guid accountId,
        Guid transferId,
        decimal amount,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken = default);
}