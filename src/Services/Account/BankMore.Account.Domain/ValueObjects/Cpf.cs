using BankMore.BuildingBlocks.Domain.Common;

namespace BankMore.Account.Domain.ValueObjects;

public sealed class Cpf : ValueObject
{
    public string Value { get; }

    private Cpf(string value)
    {
        Value = value;
    }

    public static Cpf Create(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new DomainException("CPF não pode ser vazio.");

        var normalized = Normalize(cpf);

        if (!IsValid(normalized))
            throw new DomainException("CPF inválido.");

        return new Cpf(normalized);
    }

    public static bool TryCreate(string cpf, out Cpf? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var normalized = Normalize(cpf);

        if (!IsValid(normalized))
            return false;

        result = new Cpf(normalized);
        return true;
    }

    public static string Normalize(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    public static bool IsValid(string cpf)
    {
        cpf = Normalize(cpf);

        if (cpf.Length != 11)
            return false;

        if (cpf.All(c => c == cpf[0]))
            return false;

        var numbers = cpf.Select(c => c - '0').ToArray();

        var sum1 = 0;
        for (var i = 0; i < 9; i++)
            sum1 += numbers[i] * (10 - i);

        var remainder1 = sum1 % 11;
        var digit1 = remainder1 < 2 ? 0 : 11 - remainder1;

        if (numbers[9] != digit1)
            return false;

        var sum2 = 0;
        for (var i = 0; i < 10; i++)
            sum2 += numbers[i] * (11 - i);

        var remainder2 = sum2 % 11;
        var digit2 = remainder2 < 2 ? 0 : 11 - remainder2;

        return numbers[10] == digit2;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}