using BankMore.BuildingBlocks.Domain.Common;

namespace BankMore.Transfer.Domain.Entities;

public sealed class TransferOperation : AggregateRoot
{
    public string RequestId { get; private set; } = string.Empty;

    public Guid SourceAccountId { get; private set; }

    public Guid DestinationAccountId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private TransferOperation()
    {
    }

    private TransferOperation(
        Guid id,
        string requestId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime createdAtUtc) : base(id)
    {
        RequestId = requestId;
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        Amount = amount;
        CreatedAtUtc = createdAtUtc;
    }

    public static TransferOperation Create(
        string requestId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new DomainException("O identificador da requisição é obrigatório.");

        if (sourceAccountId == Guid.Empty)
            throw new DomainException("A conta de origem é obrigatória.");

        if (destinationAccountId == Guid.Empty)
            throw new DomainException("A conta de destino é obrigatória.");

        if (amount <= 0)
            throw new DomainException("O valor da transferência deve ser maior que zero.");

        if (sourceAccountId == destinationAccountId)
            throw new DomainException("A conta de origem e a conta de destino não podem ser a mesma.");

        return new TransferOperation(
            Guid.NewGuid(),
            requestId.Trim(),
            sourceAccountId,
            destinationAccountId,
            amount,
            createdAtUtc ?? DateTime.UtcNow);
    }
}