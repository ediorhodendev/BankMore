namespace BankMore.Account.Application.DTOs;

public sealed class BalanceResponse
{
    public string AccountNumber { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public DateTime QueriedAtUtc { get; init; }

    public decimal Balance { get; init; }
}