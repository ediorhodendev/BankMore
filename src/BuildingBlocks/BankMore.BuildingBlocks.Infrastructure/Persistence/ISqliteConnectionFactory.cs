using System.Data;

namespace BankMore.BuildingBlocks.Infrastructure.Persistence;

public interface ISqliteConnectionFactory
{
    IDbConnection CreateConnection();
}