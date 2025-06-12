using Microsoft.Extensions.Logging;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;
using Pizzeria.App.Utils;

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
        var orderPrices = OrderUtils.CalculateOrderPrices(orders, products);

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

    public async Task CalculateIngredientsAmountAsync(IEnumerable<OrderItem> orders, string filePath)
    {
        var ingredients = await LoadIngredientsAsync(filePath);
        var (totalIngredients, missingProducts) = OrderUtils.CalculateIngredientsAmount(orders, ingredients);

        foreach (var (orderId, productId) in missingProducts)
        {
            logger.LogWarning("Product {ProductId} not found in ingredients file for Order {OrderId}",
                productId, orderId);
        }

        logger.LogInformation("Total ingredients required for all orders:");
        foreach (var ingredient in totalIngredients.Values.OrderBy(i => i.Name))
        {
            logger.LogInformation("{Name}: {Amount} {Unit}", ingredient.Name, ingredient.Amount, ingredient.Unit);
        }
    }

    private async Task<Dictionary<Guid, ProductIngredients>> LoadIngredientsAsync(string ingredientsFilePath)
    {
        return await LoadDataAsync<ProductIngredients>(ingredientsFilePath, "Ingredients", "ingredients");
    }

    private async Task<Dictionary<Guid, Product>> LoadProductsAsync(string productsFilePath)
    {
        return await LoadDataAsync<Product>(productsFilePath, "Products", "products");
    }

    private async Task<Dictionary<Guid, T>> LoadDataAsync<T>(string filePath, string entityTypeName, string logName) where T : class, IHasProductId
    {
        if (!File.Exists(filePath))
        {
            logger.LogError("{EntityType} file not found at {FilePath}", entityTypeName, filePath);
            throw new FileNotFoundException($"{entityTypeName} file not found at {filePath}");
        }

        var parser = parserFactory.GetParser<T>(filePath);
        var items = await parser.ParseAsync(filePath);

        var dictionary = items.ToDictionary(item => item.ProductId, item => item);
        logger.LogInformation("Loaded {Count} {LogName} from {FilePath}.", dictionary.Count, logName, filePath);

        return dictionary;
    }
}