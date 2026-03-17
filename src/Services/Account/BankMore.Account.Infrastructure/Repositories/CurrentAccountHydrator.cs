using BankMore.Account.Domain.Entities;
using System.Reflection;

namespace BankMore.Account.Infrastructure.Repositories;

internal static class CurrentAccountHydrator
{
    public static CurrentAccount Hydrate(
        Guid id,
        string accountNumber,
        string name,
        string cpf,
        string passwordHash,
        bool isActive,
        DateTime createdAtUtc,
        DateTime? deactivatedAtUtc)
    {
        var account = (CurrentAccount)Activator.CreateInstance(typeof(CurrentAccount), nonPublic: true)!;

        SetProperty(account, nameof(CurrentAccount.Id), id);
        SetProperty(account, nameof(CurrentAccount.AccountNumber), accountNumber);
        SetProperty(account, nameof(CurrentAccount.Name), name);
        SetProperty(account, nameof(CurrentAccount.Cpf), cpf);
        SetProperty(account, nameof(CurrentAccount.PasswordHash), passwordHash);
        SetProperty(account, nameof(CurrentAccount.IsActive), isActive);
        SetProperty(account, nameof(CurrentAccount.CreatedAtUtc), createdAtUtc);
        SetProperty(account, nameof(CurrentAccount.DeactivatedAtUtc), deactivatedAtUtc);

        return account;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property?.SetValue(target, value);
    }
}