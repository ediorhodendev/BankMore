using BankMore.Account.Application.Abstractions.Security;
using BankMore.BuildingBlocks.Infrastructure.Authentication;

namespace BankMore.Account.Infrastructure.Security;

public sealed class TokenProvider : ITokenProvider
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public TokenProvider(IJwtTokenGenerator jwtTokenGenerator)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public string GenerateToken(Guid accountId, string accountNumber, string name)
    {
        return _jwtTokenGenerator.GenerateToken(accountId, accountNumber, name);
    }
}