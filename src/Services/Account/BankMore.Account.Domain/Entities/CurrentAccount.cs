using BankMore.BuildingBlocks.Domain.Common;

namespace BankMore.Account.Domain.Entities;

public sealed class CurrentAccount : AggregateRoot
{
    public string AccountNumber { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Cpf { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeactivatedAtUtc { get; private set; }

    private CurrentAccount()
    {
    }

    private CurrentAccount(
        Guid id,
        string accountNumber,
        string name,
        string cpf,
        string passwordHash,
        bool isActive,
        DateTime createdAtUtc) : base(id)
    {
        AccountNumber = accountNumber;
        Name = name;
        Cpf = cpf;
        PasswordHash = passwordHash;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public static CurrentAccount Create(
        string accountNumber,
        string name,
        string cpf,
        string passwordHash,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new DomainException("O número da conta é obrigatório.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do titular é obrigatório.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("A senha da conta é obrigatória.");

        var validCpf = ValueObjects.Cpf.Create(cpf);

        return new CurrentAccount(
            Guid.NewGuid(),
            accountNumber.Trim(),
            name.Trim(),
            validCpf.Value,
            passwordHash.Trim(),
            true,
            createdAtUtc ?? DateTime.UtcNow);
    }

    public void Deactivate()
    {
        EnsureIsActive();
        IsActive = false;
        DeactivatedAtUtc = DateTime.UtcNow;
    }

    public void EnsureIsActive()
    {
        if (!IsActive)
            throw new DomainException("A conta corrente está inativa.");
    }

    public bool HasAccountNumber(string accountNumber)
    {
        return string.Equals(
            AccountNumber,
            accountNumber?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    public bool HasCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        return string.Equals(
            Cpf,
            ValueObjects.Cpf.Normalize(cpf),
            StringComparison.Ordinal);
    }
}