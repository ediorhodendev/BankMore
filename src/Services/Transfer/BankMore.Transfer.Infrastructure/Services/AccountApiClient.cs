using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankMore.Transfer.Application.Abstractions.Services;

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

        var payload = await response.Content.ReadFromJsonAsync<CurrentAccountPayload>(cancellationToken: cancellationToken);

        if (payload is null)
            return null;

        return new AccountLookupResult
        {
            AccountId = payload.AccountId,
            Name = payload.Name,
            IsActive = payload.IsActive
        };
    }

    public async Task<AccountLookupResult?> ResolveAccountByNumberAsync(
        string accountNumber,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/internal/accounts/resolve/{Uri.EscapeDataString(accountNumber)}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadFromJsonAsync<ResolveAccountPayload>(cancellationToken: cancellationToken);

        if (payload is null)
            return null;

        return new AccountLookupResult
        {
            AccountId = payload.AccountId,
            Name = payload.Name,
            IsActive = payload.IsActive
        };
    }

    public async Task<AccountMovementResult> CreateDebitAsync(
        string requestId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/movements");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Content = JsonContent.Create(new
        {
            requestId,
            amount,
            type = "D"
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return AccountMovementResult.Success();

        var error = await TryReadErrorAsync(response, cancellationToken);
        return AccountMovementResult.Failure(error.code, error.message);
    }

    public async Task<AccountMovementResult> CreateCreditByAccountIdAsync(
        string requestId,
        Guid destinationAccountId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/movements/credit");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Content = JsonContent.Create(new
        {
            requestId,
            targetAccountId = destinationAccountId,
            amount,
            type = "C"
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return AccountMovementResult.Success();

        var error = await TryReadErrorAsync(response, cancellationToken);
        return AccountMovementResult.Failure(error.code, error.message);
    }

    public async Task<AccountMovementResult> RevertDebitWithCreditAsync(
        string requestId,
        decimal amount,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/movements");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Content = JsonContent.Create(new
        {
            requestId,
            amount,
            type = "C"
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return AccountMovementResult.Success();

        var error = await TryReadErrorAsync(response, cancellationToken);
        return AccountMovementResult.Failure(error.code, error.message);
    }

    private static async Task<(string code, string message)> TryReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>(cancellationToken: cancellationToken);
            if (payload is not null)
                return (payload.Type ?? "ACCOUNT_API_ERROR", payload.Message ?? "Erro ao chamar a API de conta.");
        }
        catch
        {
        }

        return ("ACCOUNT_API_ERROR", "Erro ao chamar a API de conta.");
    }

    private sealed class CurrentAccountPayload
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class ResolveAccountPayload
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class ErrorPayload
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
    }
}