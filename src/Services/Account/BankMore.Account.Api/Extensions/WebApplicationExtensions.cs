using BankMore.Account.Infrastructure.Persistence;

namespace BankMore.Account.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeAccountDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<AccountDbInitializer>();
        await initializer.InitializeAsync();
    }
}