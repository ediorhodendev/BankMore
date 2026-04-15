namespace BankMore.Transfer.Application.DTOs;

public sealed class TransferResponse
{
    public Guid TransferId { get; init; }

    public string RequestId { get; init; } = string.Empty;

    public Guid SourceAccountId { get; init; }

    public Guid DestinationAccountId { get; init; }

    public decimal Amount { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}