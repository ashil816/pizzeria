using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Parsers;
using Pizzeria.App.Services;
using Pizzeria.App.Validators;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton<IOrderParserFactory, OrderParserFactory>()
    .AddSingleton<IOrderService, OrderService>()
    .AddSingleton<IOrderValidator, OrderValidator>();

using var host = builder.Build();


using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var orderService = services.GetRequiredService<IOrderService>();
var logger = services.GetRequiredService<ILogger<Program>>();

if (args.Length != 3)
{
    logger.LogError("No or invalid arguments provided. Please provide the paths to the orders file, products file, and ingredients file.");
    return;
}
string ordersFilePath = args[0];
string productsFilePath = args[1];
string IngredientsFilePath = args[2];

try
{
    var validOrders = await orderService.GetOrdersAsync(ordersFilePath);
    await orderService.CalculateOrderPriceAsync(validOrders, productsFilePath);
    await orderService.CalculateIngredientsAmountAsync(validOrders, IngredientsFilePath);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while processing the orders.");
}