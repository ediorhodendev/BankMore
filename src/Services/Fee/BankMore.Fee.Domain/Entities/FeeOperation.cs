using BankMore.BuildingBlocks.Domain.Common;

namespace BankMore.Fee.Domain.Entities;

public sealed class FeeOperation : AggregateRoot
{
    public Guid TransferId { get; private set; }

    public Guid AccountId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private FeeOperation()
    {
    }

    private FeeOperation(
        Guid id,
        Guid transferId,
        Guid accountId,
        decimal amount,
        DateTime createdAtUtc) : base(id)
    {
        TransferId = transferId;
        AccountId = accountId;
        Amount = amount;
        CreatedAtUtc = createdAtUtc;
    }

    public static FeeOperation Create(
        Guid transferId,
        Guid accountId,
        decimal amount,
        DateTime? createdAtUtc = null)
    {
        if (transferId == Guid.Empty)
            throw new DomainException("A transferência é obrigatória.");

        if (accountId == Guid.Empty)
            throw new DomainException("A conta é obrigatória.");

        if (amount <= 0)
            throw new DomainException("O valor da tarifa deve ser maior que zero.");

        return new FeeOperation(
            Guid.NewGuid(),
            transferId,
            accountId,
            amount,
            createdAtUtc ?? DateTime.UtcNow);
    }
}