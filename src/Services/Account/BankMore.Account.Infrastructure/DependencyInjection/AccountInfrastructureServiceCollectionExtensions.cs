using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Abstractions.Services;
using BankMore.Account.Infrastructure.Persistence;
using BankMore.Account.Infrastructure.Repositories;
using BankMore.Account.Infrastructure.Security;
using BankMore.Account.Infrastructure.Services;
using BankMore.BuildingBlocks.Contracts.Authentication;
using BankMore.BuildingBlocks.Infrastructure.Authentication;
using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.BuildingBlocks.Infrastructure.Security;
using BankMore.Transfer.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Account.Infrastructure.DependencyInjection;

public static class AccountInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAccountInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AccountDatabase")
                               ?? "Data Source=bankmore-account.db";

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<ISqliteConnectionFactory>(_ =>
            new SqliteConnectionFactory(connectionString));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        services.AddSingleton<IAccountNumberGenerator, AccountNumberGenerator>();

        services.AddScoped<AccountDbInitializer>();

        return services;
    }
}