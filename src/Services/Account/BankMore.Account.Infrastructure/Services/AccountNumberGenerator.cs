using BankMore.Account.Application.Abstractions.Services;

namespace BankMore.Account.Infrastructure.Services;

public sealed class AccountNumberGenerator : IAccountNumberGenerator
{
    public string Generate()
    {
        var random = Random.Shared.Next(100000, 999999);
        var suffix = DateTime.UtcNow.Ticks.ToString()[^4..];
        return $"{random}{suffix}";
    }
}