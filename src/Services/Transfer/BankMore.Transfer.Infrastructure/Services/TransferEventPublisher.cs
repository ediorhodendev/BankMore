using BankMore.BuildingBlocks.Contracts.Events;
using BankMore.Transfer.Application.Abstractions.Services;
using Confluent.Kafka;
using System.Text.Json;

namespace BankMore.Transfer.Infrastructure.Services;

public sealed class TransferEventPublisher : ITransferEventPublisher
{
    private readonly IProducer<string, string>? _producer;

    public TransferEventPublisher(IProducer<string, string>? producer = null)
    {
        _producer = producer;
    }

    public async Task PublishCompletedAsync(
        Guid transferId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken = default)
    {
        if (_producer is null)
            return;

        var @event = new TransferCompletedEvent
        {
            TransferId = transferId,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = amount,
            OccurredOnUtc = occurredOnUtc
        };

        var payload = JsonSerializer.Serialize(@event);

        await _producer.ProduceAsync(
            "transfers.completed",
            new Message<string, string>
            {
                Key = transferId.ToString(),
                Value = payload
            },
            cancellationToken);
    }
}