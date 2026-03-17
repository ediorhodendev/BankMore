using BankMore.Account.Application.Abstractions.Security;
using BankMore.BuildingBlocks.Infrastructure.Security;

namespace BankMore.Account.Infrastructure.Security;

public sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string Hash(string password)
    {
        return _passwordHasher.Hash(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return _passwordHasher.Verify(password, passwordHash);
    }
}