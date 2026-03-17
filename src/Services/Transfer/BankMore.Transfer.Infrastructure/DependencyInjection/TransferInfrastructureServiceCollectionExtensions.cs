using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.Transfer.Application.Abstractions.Persistence;
using BankMore.Transfer.Application.Abstractions.Security;
using BankMore.Transfer.Application.Abstractions.Services;
using BankMore.Transfer.Infrastructure.Persistence;
using BankMore.Transfer.Infrastructure.Repositories;
using BankMore.Transfer.Infrastructure.Security;
using BankMore.Transfer.Infrastructure.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Transfer.Infrastructure.DependencyInjection;

public static class TransferInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddTransferInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TransferDatabase")
                               ?? "Data Source=bankmore-transfer.db";

        services.AddSingleton<ISqliteConnectionFactory>(_ =>
            new SqliteConnectionFactory(connectionString));

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<ITransferIdempotencyService, TransferIdempotencyService>();

        var accountApiBaseUrl = configuration["Services:AccountApiBaseUrl"] ?? "https://localhost:5001/";
        services.AddHttpClient<IAccountApiClient, AccountApiClient>(client =>
        {
            client.BaseAddress = new Uri(accountApiBaseUrl);
        });

        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"];
        if (!string.IsNullOrWhiteSpace(kafkaBootstrapServers))
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaBootstrapServers
            };

            services.AddSingleton<IProducer<string, string>>(_ =>
                new ProducerBuilder<string, string>(producerConfig).Build());
        }

        services.AddScoped<ITransferEventPublisher, TransferEventPublisher>();
        services.AddScoped<TransferDbInitializer>();

        return services;
    }
}