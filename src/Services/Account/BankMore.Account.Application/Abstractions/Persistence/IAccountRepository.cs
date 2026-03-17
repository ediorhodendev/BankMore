using BankMore.Account.Domain.Entities;

namespace BankMore.Account.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task AddAsync(CurrentAccount account, CancellationToken cancellationToken = default);

    Task UpdateAsync(CurrentAccount account, CancellationToken cancellationToken = default);

    Task<CurrentAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CurrentAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task<CurrentAccount?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
}