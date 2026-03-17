using BankMore.Fee.Domain.Entities;

namespace BankMore.Fee.Application.Abstractions.Persistence;

public interface IFeeRepository
{
    Task AddAsync(FeeOperation fee, CancellationToken cancellationToken = default);

    Task<FeeOperation?> GetByTransferIdAsync(Guid transferId, CancellationToken cancellationToken = default);
}