using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.Transfer.Application.Abstractions.Persistence;
using BankMore.Transfer.Domain.Entities;
using Dapper;
using System.Globalization;

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
                source_account_number,
                destination_account_id,
                destination_account_number,
                amount,
                created_at_utc
            )
            VALUES
            (
                @Id,
                @RequestId,
                @SourceAccountId,
                @SourceAccountNumber,
                @DestinationAccountId,
                @DestinationAccountNumber,
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
                transfer.SourceAccountNumber,
                DestinationAccountId = transfer.DestinationAccountId.ToString(),
                transfer.DestinationAccountNumber,
                transfer.Amount,
                CreatedAtUtc = transfer.CreatedAtUtc.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    public async Task<TransferOperation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id                          AS Id,
                request_id                  AS RequestId,
                source_account_id           AS SourceAccountId,
                source_account_number       AS SourceAccountNumber,
                destination_account_id      AS DestinationAccountId,
                destination_account_number  AS DestinationAccountNumber,
                amount                      AS Amount,
                created_at_utc              AS CreatedAtUtc
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
                id                          AS Id,
                request_id                  AS RequestId,
                source_account_id           AS SourceAccountId,
                source_account_number       AS SourceAccountNumber,
                destination_account_id      AS DestinationAccountId,
                destination_account_number  AS DestinationAccountNumber,
                amount                      AS Amount,
                created_at_utc              AS CreatedAtUtc
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
        return TransferOperationHydrator.Hydrate(
            id: Guid.Parse(row.Id),
            requestId: row.RequestId,
            sourceAccountId: Guid.Parse(row.SourceAccountId),
            sourceAccountNumber: row.SourceAccountNumber,
            destinationAccountId: Guid.Parse(row.DestinationAccountId),
            destinationAccountNumber: row.DestinationAccountNumber,
            amount: row.Amount,
            createdAtUtc: DateTime.Parse(row.CreatedAtUtc, null, DateTimeStyles.RoundtripKind));
    }

    private sealed class TransferRow
    {
        public string Id { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string SourceAccountId { get; set; } = string.Empty;
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountId { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CreatedAtUtc { get; set; } = string.Empty;
    }
}