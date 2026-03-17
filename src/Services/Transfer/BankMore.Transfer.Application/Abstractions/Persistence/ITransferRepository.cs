using BankMore.Transfer.Domain.Entities;

namespace BankMore.Transfer.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task AddAsync(TransferOperation transfer, CancellationToken cancellationToken = default);

    Task<TransferOperation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransferOperation?> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);
}