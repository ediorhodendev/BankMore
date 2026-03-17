using BankMore.Transfer.Domain.Entities;
using System.Reflection;

namespace BankMore.Transfer.Infrastructure.Repositories;

internal static class TransferOperationHydrator
{
    public static TransferOperation Hydrate(
        Guid id,
        string requestId,
        Guid sourceAccountId,
        string sourceAccountNumber,
        Guid destinationAccountId,
        string destinationAccountNumber,
        decimal amount,
        DateTime createdAtUtc)
    {
        var transfer = (TransferOperation)Activator.CreateInstance(typeof(TransferOperation), nonPublic: true)!;

        SetProperty(transfer, nameof(TransferOperation.Id), id);
        SetProperty(transfer, nameof(TransferOperation.RequestId), requestId);
        SetProperty(transfer, nameof(TransferOperation.SourceAccountId), sourceAccountId);
        SetProperty(transfer, nameof(TransferOperation.SourceAccountNumber), sourceAccountNumber);
        SetProperty(transfer, nameof(TransferOperation.DestinationAccountId), destinationAccountId);
        SetProperty(transfer, nameof(TransferOperation.DestinationAccountNumber), destinationAccountNumber);
        SetProperty(transfer, nameof(TransferOperation.Amount), amount);
        SetProperty(transfer, nameof(TransferOperation.CreatedAtUtc), createdAtUtc);

        return transfer;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property?.SetValue(target, value);
    }
}