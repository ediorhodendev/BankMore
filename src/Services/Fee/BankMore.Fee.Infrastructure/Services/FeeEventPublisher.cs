using BankMore.BuildingBlocks.Contracts.Events;
using BankMore.Fee.Application.Abstractions.Services;
using Confluent.Kafka;
using System.Text.Json;

namespace BankMore.Fee.Infrastructure.Services;

public sealed class FeeEventPublisher : IFeeEventPublisher
{
    private readonly IProducer<string, string>? _producer;

    public FeeEventPublisher(IProducer<string, string>? producer = null)
    {
        _producer = producer;
    }

    public async Task PublishCompletedAsync(
        Guid feeId,
        Guid accountId,
        Guid transferId,
        decimal amount,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken = default)
    {
        if (_producer is null)
            return;

        var @event = new FeeCompletedEvent
        {
            FeeId = feeId,
            AccountId = accountId,
            TransferId = transferId,
            Amount = amount,
            OccurredOnUtc = occurredOnUtc
        };

        var payload = JsonSerializer.Serialize(@event);

        await _producer.ProduceAsync(
            "fees.completed",
            new Message<string, string>
            {
                Key = feeId.ToString(),
                Value = payload
            },
            cancellationToken);
    }
}