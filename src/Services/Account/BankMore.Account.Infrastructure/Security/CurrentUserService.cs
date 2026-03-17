using BankMore.Account.Application.Abstractions.Security;
using BankMore.BuildingBlocks.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;

namespace BankMore.Transfer.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? AccountId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null)
                return null;

            return new CurrentUser(principal).AccountId;
        }
    }

    public string? AccountNumber
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null)
                return null;

            return new CurrentUser(principal).AccountNumber;
        }
    }

    public string? Name
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null)
                return null;

            return new CurrentUser(principal).Name;
        }
    }

    public string? BearerToken
    {
        get
        {
            var authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authorization))
                return null;

            const string prefix = "Bearer ";
            return authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? authorization.Substring(prefix.Length).Trim()
                : authorization.Trim();
        }
    }
}