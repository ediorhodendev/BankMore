using BankMore.Account.Domain.Enums;
using BankMore.BuildingBlocks.Domain.Common;

namespace BankMore.Account.Domain.Entities;

public sealed class Movement : Entity
{
    public Guid CurrentAccountId { get; private set; }

    public string RequestId { get; private set; } = string.Empty;

    public MovementType Type { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Movement()
    {
    }

    private Movement(
        Guid id,
        Guid currentAccountId,
        string requestId,
        MovementType type,
        decimal amount,
        DateTime createdAtUtc) : base(id)
    {
        CurrentAccountId = currentAccountId;
        RequestId = requestId;
        Type = type;
        Amount = amount;
        CreatedAtUtc = createdAtUtc;
    }

    public static Movement Create(
        Guid currentAccountId,
        string requestId,
        MovementType type,
        decimal amount,
        DateTime? createdAtUtc = null)
    {
        if (currentAccountId == Guid.Empty)
            throw new DomainException("A conta corrente é obrigatória.");

        if (string.IsNullOrWhiteSpace(requestId))
            throw new DomainException("O identificador da requisição é obrigatório.");

        if (amount <= 0)
            throw new DomainException("O valor da movimentação deve ser maior que zero.");

        if (!Enum.IsDefined(typeof(MovementType), type))
            throw new DomainException("O tipo da movimentação é inválido.");

        return new Movement(
            Guid.NewGuid(),
            currentAccountId,
            requestId.Trim(),
            type,
            amount,
            createdAtUtc ?? DateTime.UtcNow);
    }

    public bool IsCredit()
    {
        return Type == MovementType.Credit;
    }

    public bool IsDebit()
    {
        return Type == MovementType.Debit;
    }

    public decimal SignedAmount()
    {
        return Type == MovementType.Credit ? Amount : Amount * -1;
    }
}