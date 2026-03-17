namespace BankMore.BuildingBlocks.Infrastructure.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid accountId, string accountNumber, string name);
}