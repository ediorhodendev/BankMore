using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.Fee.Application.Abstractions.Persistence;
using BankMore.Fee.Domain.Entities;
using Dapper;
using System.Globalization;

namespace BankMore.Fee.Infrastructure.Repositories;

public sealed class FeeRepository : IFeeRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public FeeRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(FeeOperation fee, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO fee_operation
            (
                id,
                transfer_id,
                account_id,
                amount,
                created_at_utc
            )
            VALUES
            (
                @Id,
                @TransferId,
                @AccountId,
                @Amount,
                @CreatedAtUtc
            );
            """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = fee.Id.ToString(),
                TransferId = fee.TransferId.ToString(),
                AccountId = fee.AccountId.ToString(),
                fee.Amount,
                CreatedAtUtc = fee.CreatedAtUtc.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    public async Task<FeeOperation?> GetByTransferIdAsync(Guid transferId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id              AS Id,
                transfer_id     AS TransferId,
                account_id      AS AccountId,
                amount          AS Amount,
                created_at_utc  AS CreatedAtUtc
            FROM fee_operation
            WHERE transfer_id = @TransferId
            LIMIT 1;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<FeeRow>(new CommandDefinition(
            sql,
            new { TransferId = transferId.ToString() },
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    private static FeeOperation Map(FeeRow row)
    {
        return FeeOperationHydrator.Hydrate(
            id: Guid.Parse(row.Id),
            transferId: Guid.Parse(row.TransferId),
            accountId: Guid.Parse(row.AccountId),
            amount: row.Amount,
            createdAtUtc: DateTime.Parse(row.CreatedAtUtc, null, DateTimeStyles.RoundtripKind));
    }

    private sealed class FeeRow
    {
        public string Id { get; set; } = string.Empty;
        public string TransferId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CreatedAtUtc { get; set; } = string.Empty;
    }
}