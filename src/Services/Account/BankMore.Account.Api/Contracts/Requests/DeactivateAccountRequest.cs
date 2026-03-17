namespace BankMore.Account.Api.Contracts.Requests;

public sealed class DeactivateAccountRequest
{
    public string Password { get; set; } = string.Empty;
}