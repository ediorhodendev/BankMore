using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.Transfer.Application.Abstractions.Persistence;
using Dapper;

namespace BankMore.Transfer.Infrastructure.Repositories;

public sealed class TransferIdempotencyService : ITransferIdempotencyService
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public TransferIdempotencyService(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ExistsAsync(string scope, string requestId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1
                FROM idempotency
                WHERE scope = @Scope AND request_id = @RequestId
            );
            """;

        using var connection = _connectionFactory.CreateConnection();

        var exists = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            sql,
            new { Scope = scope, RequestId = requestId },
            cancellationToken: cancellationToken));

        return exists == 1;
    }

    public async Task RegisterAsync(string scope, string requestId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO idempotency
            (
                id,
                scope,
                request_id,
                created_at_utc
            )
            VALUES
            (
                @Id,
                @Scope,
                @RequestId,
                @CreatedAtUtc
            );
            """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = Guid.NewGuid().ToString(),
                Scope = scope,
                RequestId = requestId,
                CreatedAtUtc = DateTime.UtcNow.ToString("O")
            },
            cancellationToken: cancellationToken));
    }
}