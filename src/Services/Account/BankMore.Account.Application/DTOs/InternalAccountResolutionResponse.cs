namespace BankMore.Account.Application.DTOs;

public sealed class InternalAccountResolutionResponse
{
    public Guid AccountId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}