namespace BankMore.Account.Application.DTOs;

public sealed class CreateAccountResponse
{
    public Guid AccountId { get; init; }

    public string AccountNumber { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}