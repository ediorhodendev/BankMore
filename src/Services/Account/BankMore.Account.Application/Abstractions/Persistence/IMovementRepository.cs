using BankMore.Account.Domain.Entities;

namespace BankMore.Account.Application.Abstractions.Persistence;

public interface IMovementRepository
{
    Task AddAsync(Movement movement, CancellationToken cancellationToken = default);

    Task<decimal> GetBalanceAsync(Guid currentAccountId, CancellationToken cancellationToken = default);
}