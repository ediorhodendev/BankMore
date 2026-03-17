using BankMore.Fee.Domain.Entities;
using System.Reflection;

namespace BankMore.Fee.Infrastructure.Repositories;

internal static class FeeOperationHydrator
{
    public static FeeOperation Hydrate(
        Guid id,
        Guid transferId,
        Guid accountId,
        decimal amount,
        DateTime createdAtUtc)
    {
        var fee = (FeeOperation)Activator.CreateInstance(typeof(FeeOperation), nonPublic: true)!;

        SetProperty(fee, nameof(FeeOperation.Id), id);
        SetProperty(fee, nameof(FeeOperation.TransferId), transferId);
        SetProperty(fee, nameof(FeeOperation.AccountId), accountId);
        SetProperty(fee, nameof(FeeOperation.Amount), amount);
        SetProperty(fee, nameof(FeeOperation.CreatedAtUtc), createdAtUtc);

        return fee;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property?.SetValue(target, value);
    }
}