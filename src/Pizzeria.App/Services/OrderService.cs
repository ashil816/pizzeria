using Microsoft.Extensions.Logging;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;

namespace Pizzeria.App.Services;

public class OrderService(IOrderParserFactory parserFactory, IOrderValidator orderValidator, ILogger<OrderService> logger) : IOrderService
{
    public async Task<IEnumerable<OrderItem>> GetOrdersAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            logger.LogError("Error: File not found at {OrdersFile}", filePath);
            return [];
        }

        var parser = parserFactory.GetParser<OrderItem>(filePath);
        var orders = await parser.ParseAsync(filePath);
        logger.LogInformation("Parsed {Count} orders from {OrdersFile}.", orders.Count(), filePath);

        var validOrders = await orderValidator.ValidateOrdersAsync(orders);
        logger.LogInformation("Validated {Count} orders from {OrdersFile}.", validOrders.Count(), filePath);

        return validOrders;
    }

    public async Task CalculateOrderPriceAsync(IEnumerable<OrderItem> orders)
    {
        // get Products.jason in Data folder and parse it
        var filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "Products.json");
        var parser = parserFactory.GetParser<Product>(filePath);
        var products = await parser.ParseAsync(filePath);

        var orderGroups = orders.GroupBy(o => o.OrderId);
        foreach (var group in orderGroups)
        {
            logger.LogInformation("Order ID: {OrderId} with {Count} items.", group.Key, group.Count());
            decimal totalPrice = 0;
            foreach (var order in group)
            {
                var product = products.FirstOrDefault(p => p.ProductId == order.ProductId);
                if (product == null)
                {
                    logger.LogWarning("Product with ID {ProductId} not found in products list.", order.ProductId);
                    continue;
                }
                totalPrice += product.Price * order.Quantity;
            }
            logger.LogInformation("Total price for Order ID {OrderId}: {TotalPrice}.", group.Key, totalPrice);
        }

    }
}