namespace BankMore.Transfer.Application.Abstractions.Services;

public interface IAccountApiClient
{
    Task<AccountLookupResult?> GetCurrentAccountAsync(
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<AccountLookupResult?> GetAccountByNumberAsync(
        string accountNumber,
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<AccountMovementResult> CreateDebitAsync(
        string requestId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<AccountMovementResult> CreateCreditAsync(
        string requestId,
        string destinationAccountNumber,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<AccountMovementResult> RevertDebitWithCreditAsync(
        string requestId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default);
}

public sealed class AccountLookupResult
{
    public Guid AccountId { get; init; }

    public string AccountNumber { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal Balance { get; init; }
}

public sealed class AccountMovementResult
{
    public bool IsSuccess { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static AccountMovementResult Success()
        => new() { IsSuccess = true };

    public static AccountMovementResult Failure(string errorCode, string errorMessage)
        => new()
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
}