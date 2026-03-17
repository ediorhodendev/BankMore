namespace BankMore.Account.Application.Abstractions.Persistence;

public interface IIdempotencyService
{
    Task<bool> ExistsAsync(string scope, string requestId, CancellationToken cancellationToken = default);

    Task RegisterAsync(string scope, string requestId, CancellationToken cancellationToken = default);
}