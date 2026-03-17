namespace BankMore.BuildingBlocks.Contracts.Events;

public sealed class TransferCompletedEvent
{
    public Guid TransferId { get; set; }

    public Guid SourceAccountId { get; set; }

    public Guid DestinationAccountId { get; set; }

    public decimal Amount { get; set; }

    public DateTime OccurredOnUtc { get; set; }
}