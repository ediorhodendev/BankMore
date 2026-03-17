using BankMore.BuildingBlocks.Contracts.Events;
using BankMore.Fee.Application.Features.ProcessFee;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BankMore.Fee.Worker.Consumers;

public sealed class TransferCompletedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TransferCompletedConsumer> _logger;

    public TransferCompletedConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<TransferCompletedConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Kafka BootstrapServers não configurado. Consumer não será iniciado.");
            return;
        }

        var groupId = _configuration["Kafka:GroupId"] ?? "bankmore-fee-worker";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("transfers.completed");

        _logger.LogInformation("Consumer de transferências iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);

                if (consumeResult?.Message?.Value is null)
                    continue;

                var transferEvent = JsonSerializer.Deserialize<TransferCompletedEvent>(consumeResult.Message.Value);

                if (transferEvent is null)
                    continue;

                var feeAmount = ResolveFeeAmount(_configuration);

                using var scope = _serviceProvider.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await sender.Send(new ProcessFeeCommand(
                    transferEvent.TransferId,
                    transferEvent.SourceAccountId,
                    feeAmount), stoppingToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning(
                        "Falha ao processar tarifa da transferência {TransferId}: {ErrorCode} - {ErrorMessage}",
                        transferEvent.TransferId,
                        result.Error.Code,
                        result.Error.Message);
                }
                else
                {
                    _logger.LogInformation(
                        "Tarifa processada com sucesso para a transferência {TransferId}. FeeId: {FeeId}",
                        transferEvent.TransferId,
                        result.Value.FeeId);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir evento de transferência.");
            }
        }

        _logger.LogInformation("Consumer de transferências finalizado.");
    }

    private static decimal ResolveFeeAmount(IConfiguration configuration)
    {
        var configuredValue = configuration["Fee:Amount"];

        if (decimal.TryParse(configuredValue, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var value) && value > 0)
        {
            return value;
        }

        return 5.00m;
    }
}