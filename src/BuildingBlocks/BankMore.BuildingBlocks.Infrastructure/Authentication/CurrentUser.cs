using System.Security.Claims;

namespace BankMore.BuildingBlocks.Infrastructure.Authentication;

public sealed class CurrentUser
{
    public CurrentUser(ClaimsPrincipal principal)
    {
        Principal = principal;
    }

    public ClaimsPrincipal Principal { get; }

    public Guid? AccountId
    {
        get
        {
            var value =
                Principal.FindFirst("account_id")?.Value ??
                Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                Principal.FindFirst(ClaimTypes.Sid)?.Value ??
                Principal.FindFirst("sub")?.Value;

            return Guid.TryParse(value, out var accountId) ? accountId : null;
        }
    }

    public string? AccountNumber =>
        Principal.FindFirst("account_number")?.Value;

    public string? Name =>
        Principal.Identity?.Name ??
        Principal.FindFirst(ClaimTypes.Name)?.Value;
}