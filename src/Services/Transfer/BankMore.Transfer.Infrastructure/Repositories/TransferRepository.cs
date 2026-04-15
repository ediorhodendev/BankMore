using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.Transfer.Application.Abstractions.Persistence;
using BankMore.Transfer.Domain.Entities;
using Dapper;

namespace BankMore.Transfer.Infrastructure.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public TransferRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(TransferOperation transfer, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO transfer_operation
            (
                id,
                request_id,
                source_account_id,
                destination_account_id,
                amount,
                created_at_utc
            )
            VALUES
            (
                @Id,
                @RequestId,
                @SourceAccountId,
                @DestinationAccountId,
                @Amount,
                @CreatedAtUtc
            );
            """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = transfer.Id.ToString(),
                transfer.RequestId,
                SourceAccountId = transfer.SourceAccountId.ToString(),
                DestinationAccountId = transfer.DestinationAccountId.ToString(),
                transfer.Amount,
                CreatedAtUtc = transfer.CreatedAtUtc.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    public async Task<TransferOperation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id                     AS Id,
                request_id             AS RequestId,
                source_account_id      AS SourceAccountId,
                destination_account_id AS DestinationAccountId,
                amount                 AS Amount,
                created_at_utc         AS CreatedAtUtc
            FROM transfer_operation
            WHERE id = @Id
            LIMIT 1;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<TransferRow>(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<TransferOperation?> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id                     AS Id,
                request_id             AS RequestId,
                source_account_id      AS SourceAccountId,
                destination_account_id AS DestinationAccountId,
                amount                 AS Amount,
                created_at_utc         AS CreatedAtUtc
            FROM transfer_operation
            WHERE request_id = @RequestId
            LIMIT 1;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<TransferRow>(new CommandDefinition(
            sql,
            new { RequestId = requestId },
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    private static TransferOperation Map(TransferRow row)
    {
        return typeof(TransferOperation)
            .GetMethod("Create")!
            .Invoke(null, new object?[]
            {
                row.RequestId,
                Guid.Parse(row.SourceAccountId),
                Guid.Parse(row.DestinationAccountId),
                row.Amount,
                DateTime.Parse(row.CreatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind)
            }) as TransferOperation
            ?? throw new InvalidOperationException("Não foi possível mapear a transferência.");
    }

    private sealed class TransferRow
    {
        public string Id { get; init; } = string.Empty;
        public string RequestId { get; init; } = string.Empty;
        public string SourceAccountId { get; init; } = string.Empty;
        public string DestinationAccountId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string CreatedAtUtc { get; init; } = string.Empty;
    }
}