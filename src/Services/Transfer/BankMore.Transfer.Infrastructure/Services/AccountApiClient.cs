using BankMore.Transfer.Application.Abstractions.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankMore.Transfer.Infrastructure.Services;

public sealed class AccountApiClient : IAccountApiClient
{
    private readonly HttpClient _httpClient;

    public AccountApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AccountLookupResult?> GetCurrentAccountAsync(
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/accounts/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadFromJsonAsync<AccountSummaryResponse>(cancellationToken: cancellationToken);

        if (payload is null)
            return null;

        return new AccountLookupResult
        {
            AccountId = payload.AccountId,
            AccountNumber = payload.AccountNumber,
            Name = payload.Name,
            Balance = 0
        };
    }

    public async Task<AccountLookupResult?> GetAccountByNumberAsync(
        string accountNumber,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/accounts/by-number/{Uri.EscapeDataString(accountNumber)}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadFromJsonAsync<AccountSummaryResponse>(cancellationToken: cancellationToken);

        if (payload is null)
            return null;

        return new AccountLookupResult
        {
            AccountId = payload.AccountId,
            AccountNumber = payload.AccountNumber,
            Name = payload.Name,
            Balance = 0
        };
    }

    public async Task<AccountMovementResult> CreateDebitAsync(
        string requestId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        return await SendMovementAsync(
            requestId: requestId,
            accountNumber: null,
            amount: amount,
            type: "D",
            bearerToken: bearerToken,
            cancellationToken: cancellationToken);
    }

    public async Task<AccountMovementResult> CreateCreditAsync(
        string requestId,
        string destinationAccountNumber,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        return await SendMovementAsync(
            requestId: requestId,
            accountNumber: destinationAccountNumber,
            amount: amount,
            type: "C",
            bearerToken: bearerToken,
            cancellationToken: cancellationToken);
    }

    public async Task<AccountMovementResult> RevertDebitWithCreditAsync(
        string requestId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        return await SendMovementAsync(
            requestId: requestId,
            accountNumber: null,
            amount: amount,
            type: "C",
            bearerToken: bearerToken,
            cancellationToken: cancellationToken);
    }

    private async Task<AccountMovementResult> SendMovementAsync(
        string requestId,
        string? accountNumber,
        decimal amount,
        string type,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/movements");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Content = JsonContent.Create(new
        {
            requestId,
            accountNumber,
            amount,
            type
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return AccountMovementResult.Success();

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: cancellationToken);

        return AccountMovementResult.Failure(
            error?.Type ?? "ACCOUNT_API_ERROR",
            error?.Message ?? "Falha ao processar movimentação na API de conta.");
    }

    private sealed class AccountSummaryResponse
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class ApiErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}