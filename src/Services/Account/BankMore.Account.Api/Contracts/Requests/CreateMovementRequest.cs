namespace BankMore.Account.Api.Contracts.Requests;

public sealed class CreateMovementRequest
{
    public string RequestId { get; set; } = string.Empty;

    public string? AccountNumber { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;
}