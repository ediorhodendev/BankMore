using BankMore.Fee.Application.Features.ProcessFee;
using BankMore.Fee.Infrastructure.DependencyInjection;
using BankMore.Fee.Infrastructure.Persistence;
using BankMore.Fee.Worker.Consumers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessFeeCommand).Assembly);
});

builder.Services.AddFeeInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TransferCompletedConsumer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<FeeDbInitializer>();
    await initializer.InitializeAsync();
}

await host.RunAsync();