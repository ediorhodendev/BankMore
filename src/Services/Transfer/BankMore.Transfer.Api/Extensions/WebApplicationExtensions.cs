using BankMore.Transfer.Infrastructure.Persistence;

namespace BankMore.Transfer.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeTransferDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<TransferDbInitializer>();
        await initializer.InitializeAsync();
    }
}