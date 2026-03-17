namespace BankMore.Account.Application.DTOs;

public sealed class LoginResponse
{
    public Guid AccountId { get; init; }

    public string AccountNumber { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;
}