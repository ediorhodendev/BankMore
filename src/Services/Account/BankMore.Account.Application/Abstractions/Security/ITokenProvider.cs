namespace BankMore.Account.Application.Abstractions.Security;

public interface ITokenProvider
{
    string GenerateToken(Guid accountId, string accountNumber, string name);
}