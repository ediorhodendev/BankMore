namespace BankMore.Transfer.Application.DTOs;

public sealed class TransferResponse
{
    public Guid TransferId { get; init; }

    public string RequestId { get; init; } = string.Empty;

    public Guid SourceAccountId { get; init; }

    public string SourceAccountNumber { get; init; } = string.Empty;

    public Guid DestinationAccountId { get; init; }

    public string DestinationAccountNumber { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}