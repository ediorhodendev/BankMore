namespace BankMore.BuildingBlocks.Contracts.Events;

public sealed class FeeCompletedEvent
{
    public Guid FeeId { get; set; }

    public Guid AccountId { get; set; }

    public Guid TransferId { get; set; }

    public decimal Amount { get; set; }

    public DateTime OccurredOnUtc { get; set; }
}