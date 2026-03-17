namespace BankMore.Account.Application.DTOs;

public sealed class AccountSummaryResponse
{
    public Guid AccountId { get; init; }

    public string AccountNumber { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}