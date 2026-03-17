namespace BankMore.Account.Application.Abstractions.Security;

public interface ICurrentUserService
{
    Guid? AccountId { get; }

    string? AccountNumber { get; }

    string? Name { get; }
}