using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Domain.Entities;
using BankMore.BuildingBlocks.Infrastructure.Persistence;
using Dapper;
using System.Globalization;

namespace BankMore.Account.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public AccountRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(CurrentAccount account, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO current_account
            (
                id,
                account_number,
                name,
                cpf,
                password_hash,
                is_active,
                created_at_utc,
                deactivated_at_utc
            )
            VALUES
            (
                @Id,
                @AccountNumber,
                @Name,
                @Cpf,
                @PasswordHash,
                @IsActive,
                @CreatedAtUtc,
                @DeactivatedAtUtc
            );
            """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = account.Id.ToString(),
                account.AccountNumber,
                account.Name,
                account.Cpf,
                account.PasswordHash,
                IsActive = account.IsActive ? 1 : 0,
                CreatedAtUtc = account.CreatedAtUtc.ToString("O"),
                DeactivatedAtUtc = account.DeactivatedAtUtc?.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(CurrentAccount account, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE current_account
            SET
                account_number = @AccountNumber,
                name = @Name,
                cpf = @Cpf,
                password_hash = @PasswordHash,
                is_active = @IsActive,
                created_at_utc = @CreatedAtUtc,
                deactivated_at_utc = @DeactivatedAtUtc
            WHERE id = @Id;
            """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = account.Id.ToString(),
                account.AccountNumber,
                account.Name,
                account.Cpf,
                account.PasswordHash,
                IsActive = account.IsActive ? 1 : 0,
                CreatedAtUtc = account.CreatedAtUtc.ToString("O"),
                DeactivatedAtUtc = account.DeactivatedAtUtc?.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    public async Task<CurrentAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id                    AS Id,
                account_number        AS AccountNumber,
                name                  AS Name,
                cpf                   AS Cpf,
                password_hash         AS PasswordHash,
                is_active             AS IsActive,
                created_at_utc        AS CreatedAtUtc,
                deactivated_at_utc    AS DeactivatedAtUtc
            FROM current_account
            WHERE id = @Id
            LIMIT 1;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<AccountRow>(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<CurrentAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id                    AS Id,
                account_number        AS AccountNumber,
                name                  AS Name,
                cpf                   AS Cpf,
                password_hash         AS PasswordHash,
                is_active             AS IsActive,
                created_at_utc        AS CreatedAtUtc,
                deactivated_at_utc    AS DeactivatedAtUtc
            FROM current_account
            WHERE account_number = @AccountNumber
            LIMIT 1;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<AccountRow>(new CommandDefinition(
            sql,
            new { AccountNumber = accountNumber },
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<CurrentAccount?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id                    AS Id,
                account_number        AS AccountNumber,
                name                  AS Name,
                cpf                   AS Cpf,
                password_hash         AS PasswordHash,
                is_active             AS IsActive,
                created_at_utc        AS CreatedAtUtc,
                deactivated_at_utc    AS DeactivatedAtUtc
            FROM current_account
            WHERE cpf = @Cpf
            LIMIT 1;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<AccountRow>(new CommandDefinition(
            sql,
            new { Cpf = cpf },
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    private static CurrentAccount Map(AccountRow row)
    {
        return CurrentAccountHydrator.Hydrate(
            id: Guid.Parse(row.Id),
            accountNumber: row.AccountNumber,
            name: row.Name,
            cpf: row.Cpf,
            passwordHash: row.PasswordHash,
            isActive: row.IsActive == 1,
            createdAtUtc: DateTime.Parse(row.CreatedAtUtc, null, DateTimeStyles.RoundtripKind),
            deactivatedAtUtc: string.IsNullOrWhiteSpace(row.DeactivatedAtUtc)
                ? null
                : DateTime.Parse(row.DeactivatedAtUtc, null, DateTimeStyles.RoundtripKind));
    }

    private sealed class AccountRow
    {
        public string Id { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int IsActive { get; set; }
        public string CreatedAtUtc { get; set; } = string.Empty;
        public string? DeactivatedAtUtc { get; set; }
    }
}