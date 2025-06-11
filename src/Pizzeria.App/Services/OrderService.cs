using Microsoft.Extensions.Logging;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;

namespace Pizzeria.App.Services;

public class OrderService(IOrderParserFactory parserFactory, ILogger<OrderService> logger) : IOrderService
{
    public async Task<IEnumerable<OrderItem>> GetOrdersAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            logger.LogError("Error: File not found at {ordersFile}", filePath);
            return [];
        }

        var parser = parserFactory.GetParser(filePath);
        var orders = await parser.ParseAsync(filePath);
        logger.LogInformation("Parsed {count} orders from {ordersFile}.", orders.Count(), filePath);
        return orders;
    }

}