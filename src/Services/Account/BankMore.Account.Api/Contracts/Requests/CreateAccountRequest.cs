namespace BankMore.Account.Api.Contracts.Requests;

public sealed class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}