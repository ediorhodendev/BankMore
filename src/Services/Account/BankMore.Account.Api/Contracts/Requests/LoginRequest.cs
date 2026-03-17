namespace BankMore.Account.Api.Contracts.Requests;

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}