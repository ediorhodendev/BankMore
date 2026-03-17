using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Domain.Entities;
using BankMore.BuildingBlocks.Infrastructure.Persistence;
using Dapper;

namespace BankMore.Account.Infrastructure.Repositories;

public sealed class MovementRepository : IMovementRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public MovementRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(Movement movement, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO movement
            (
                id,
                current_account_id,
                request_id,
                type,
                amount,
                created_at_utc
            )
            VALUES
            (
                @Id,
                @CurrentAccountId,
                @RequestId,
                @Type,
                @Amount,
                @CreatedAtUtc
            );
            """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = movement.Id.ToString(),
                CurrentAccountId = movement.CurrentAccountId.ToString(),
                movement.RequestId,
                Type = (int)movement.Type,
                movement.Amount,
                CreatedAtUtc = movement.CreatedAtUtc.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    public async Task<decimal> GetBalanceAsync(Guid currentAccountId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                COALESCE(SUM(
                    CASE
                        WHEN type = 1 THEN amount
                        WHEN type = 2 THEN amount * -1
                        ELSE 0
                    END
                ), 0)
            FROM movement
            WHERE current_account_id = @CurrentAccountId;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var balance = await connection.ExecuteScalarAsync<decimal>(new CommandDefinition(
            sql,
            new { CurrentAccountId = currentAccountId.ToString() },
            cancellationToken: cancellationToken));

        return balance;
    }
}