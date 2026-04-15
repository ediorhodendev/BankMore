namespace BankMore.Account.Api.Contracts.Requests;

public sealed class InternalCreateMovementRequest
{
    public string RequestId { get; set; } = string.Empty;
    public Guid TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
}