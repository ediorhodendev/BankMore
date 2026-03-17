namespace BankMore.Transfer.Api.Contracts.Requests;

public sealed class PerformTransferRequest
{
    public string RequestId { get; set; } = string.Empty;

    public string DestinationAccountNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}