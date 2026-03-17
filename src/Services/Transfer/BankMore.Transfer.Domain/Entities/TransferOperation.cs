using BankMore.BuildingBlocks.Domain.Common;

namespace BankMore.Transfer.Domain.Entities;

public sealed class TransferOperation : AggregateRoot
{
    public string RequestId { get; private set; } = string.Empty;

    public Guid SourceAccountId { get; private set; }

    public string SourceAccountNumber { get; private set; } = string.Empty;

    public Guid DestinationAccountId { get; private set; }

    public string DestinationAccountNumber { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private TransferOperation()
    {
    }

    private TransferOperation(
        Guid id,
        string requestId,
        Guid sourceAccountId,
        string sourceAccountNumber,
        Guid destinationAccountId,
        string destinationAccountNumber,
        decimal amount,
        DateTime createdAtUtc) : base(id)
    {
        RequestId = requestId;
        SourceAccountId = sourceAccountId;
        SourceAccountNumber = sourceAccountNumber;
        DestinationAccountId = destinationAccountId;
        DestinationAccountNumber = destinationAccountNumber;
        Amount = amount;
        CreatedAtUtc = createdAtUtc;
    }

    public static TransferOperation Create(
        string requestId,
        Guid sourceAccountId,
        string sourceAccountNumber,
        Guid destinationAccountId,
        string destinationAccountNumber,
        decimal amount,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new DomainException("O identificador da requisição é obrigatório.");

        if (sourceAccountId == Guid.Empty)
            throw new DomainException("A conta de origem é obrigatória.");

        if (destinationAccountId == Guid.Empty)
            throw new DomainException("A conta de destino é obrigatória.");

        if (string.IsNullOrWhiteSpace(sourceAccountNumber))
            throw new DomainException("O número da conta de origem é obrigatório.");

        if (string.IsNullOrWhiteSpace(destinationAccountNumber))
            throw new DomainException("O número da conta de destino é obrigatório.");

        if (amount <= 0)
            throw new DomainException("O valor da transferência deve ser maior que zero.");

        if (sourceAccountId == destinationAccountId)
            throw new DomainException("A conta de origem e a conta de destino não podem ser a mesma.");

        return new TransferOperation(
            Guid.NewGuid(),
            requestId.Trim(),
            sourceAccountId,
            sourceAccountNumber.Trim(),
            destinationAccountId,
            destinationAccountNumber.Trim(),
            amount,
            createdAtUtc ?? DateTime.UtcNow);
    }
}