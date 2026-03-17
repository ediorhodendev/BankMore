using BankMore.BuildingBlocks.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Hosting;

namespace BankMore.Account.Infrastructure.Persistence;

public sealed class AccountDbInitializer
{
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly IHostEnvironment _hostEnvironment;

    public AccountDbInitializer(
        ISqliteConnectionFactory connectionFactory,
        IHostEnvironment hostEnvironment)
    {
        _connectionFactory = connectionFactory;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var sqlPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "Persistence",
            "Sql",
            "account.sql");

        if (!File.Exists(sqlPath))
            throw new FileNotFoundException("Arquivo SQL da conta não encontrado.", sqlPath);

        var script = await File.ReadAllTextAsync(sqlPath, cancellationToken);

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(script, cancellationToken: cancellationToken));
    }
}