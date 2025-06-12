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

    public async Task CalculateOrderPriceAsync(IEnumerable<OrderItem> orders, string filePath)
    {

        var products = await LoadProductsAsync(filePath);
        var orderPrices = CalculateOrderPricesInternal(orders, products);

        foreach (var orderPrice in orderPrices)
        {
            logger.LogInformation("Order ID: {OrderId} with {Count} items.", orderPrice.OrderId, orderPrice.ItemCount);

            if (orderPrice.MissingProductIds.Count > 0)
            {
                logger.LogWarning("Order ID {OrderId} has {Count} missing products: {MissingProducts}",
                    orderPrice.OrderId, orderPrice.MissingProductIds.Count,
                    string.Join(", ", orderPrice.MissingProductIds));
            }

            logger.LogInformation("Total price for Order ID {OrderId}: {TotalPrice}.", orderPrice.OrderId, orderPrice.TotalPrice);
        }
    }

    private async Task<Dictionary<Guid, Product>> LoadProductsAsync(string productsFilePath)
    {

        if (!File.Exists(productsFilePath))
        {
            logger.LogError("Products file not found at {ProductsFile}", productsFilePath);
            throw new FileNotFoundException($"Products file not found at {productsFilePath}");
        }

        var parser = parserFactory.GetParser<Product>(productsFilePath);
        var products = await parser.ParseAsync(productsFilePath);

        var productsDictionary = products.ToDictionary(p => p.ProductId, p => p);
        logger.LogInformation("Loaded {Count} products from {ProductsFile}.", productsDictionary.Count, productsFilePath);

        return productsDictionary;

    }

    private static List<OrderPriceCalculation> CalculateOrderPricesInternal(
        IEnumerable<OrderItem> orders,
        Dictionary<Guid, Product> products)
    {
        var orderPrices = new List<OrderPriceCalculation>();
        var orderGroups = orders.GroupBy(o => o.OrderId);

        foreach (var group in orderGroups)
        {
            decimal totalPrice = 0;
            var missingProducts = new List<Guid>();

            foreach (var order in group)
            {
                if (products.TryGetValue(order.ProductId, out var product))
                {
                    totalPrice += product.Price * order.Quantity;
                }
                else
                {
                    missingProducts.Add(order.ProductId);
                }
            }

            var calculation = new OrderPriceCalculation(
                group.Key,
                group.Count(),
                totalPrice,
                missingProducts
            );

            orderPrices.Add(calculation);
        }

        return orderPrices;
    }
}