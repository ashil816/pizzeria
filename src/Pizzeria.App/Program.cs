using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Parsers;
using Pizzeria.App.Services;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IOrderParserFactory, OrderParserFactory>();
builder.Services.AddSingleton<IOrderService, OrderService>();

using var host = builder.Build();


using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var orderService = services.GetRequiredService<IOrderService>();
var logger = services.GetRequiredService<ILogger<Program>>();

if (args.Length == 0)
{
    logger.LogError("No file path provided. Please specify the path to the order file.");
    return;
}
string filePath = args[0];

try
{
    var orders = await orderService.GetOrdersAsync(filePath);
    logger.LogInformation("Successfully processed {count} orders from {filePath}.", orders.Count(), filePath);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while processing the orders.");
}
finally
{
    await host.StopAsync();
}

await host.RunAsync();