using BankMore.BuildingBlocks.Infrastructure.Persistence;
using BankMore.Fee.Application.Abstractions.Persistence;
using BankMore.Fee.Application.Abstractions.Services;
using BankMore.Fee.Infrastructure.Persistence;
using BankMore.Fee.Infrastructure.Repositories;
using BankMore.Fee.Infrastructure.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Fee.Infrastructure.DependencyInjection;

public static class FeeInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddFeeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FeeDatabase")
                               ?? "Data Source=bankmore-fee.db";

        services.AddSingleton<ISqliteConnectionFactory>(_ =>
            new SqliteConnectionFactory(connectionString));

        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IFeeEventPublisher, FeeEventPublisher>();

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

        services.AddScoped<FeeDbInitializer>();

        return services;
    }
}