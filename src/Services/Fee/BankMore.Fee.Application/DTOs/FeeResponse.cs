namespace BankMore.Fee.Application.DTOs;

public sealed class FeeResponse
{
    public Guid FeeId { get; init; }

    public Guid TransferId { get; init; }

    public Guid AccountId { get; init; }

    public decimal Amount { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}