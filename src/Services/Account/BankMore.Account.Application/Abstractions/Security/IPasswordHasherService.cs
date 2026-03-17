namespace BankMore.Account.Application.Abstractions.Security;

public interface IPasswordHasherService
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}