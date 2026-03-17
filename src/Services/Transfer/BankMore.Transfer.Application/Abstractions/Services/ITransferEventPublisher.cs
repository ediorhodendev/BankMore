namespace BankMore.Transfer.Application.Abstractions.Services;

public interface ITransferEventPublisher
{
    Task PublishCompletedAsync(
        Guid transferId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken = default);
}