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

    public async Task CalculateIngredientsAmountAsync(IEnumerable<OrderItem> orders, string filePath)
    {
        var ingredients = await LoadIngredientsAsync(filePath);
        var totalIngredients = new Dictionary<string, Ingredient>();

        foreach (var order in orders)
        {
            if (ingredients.TryGetValue(order.ProductId, out var productIngredients))
            {
                foreach (var ingredient in productIngredients.Ingredients)
                {
                    var key = $"{ingredient.Name}_{ingredient.Unit}";
                    var totalAmount = ingredient.Amount * order.Quantity;

                    if (totalIngredients.TryGetValue(key, out var existingIngredient))
                    {
                        totalIngredients[key] = existingIngredient with { Amount = existingIngredient.Amount + totalAmount };
                    }
                    else
                    {
                        totalIngredients[key] = new Ingredient(ingredient.Name, totalAmount, ingredient.Unit);
                    }
                }
            }
            else
            {
                logger.LogWarning("Product {ProductId} not found in ingredients file for Order {OrderId}",
                    order.ProductId, order.OrderId);
            }
        }

        logger.LogInformation("Total ingredients required for all orders:");
        foreach (var ingredient in totalIngredients.Values.OrderBy(i => i.Name))
        {
            logger.LogInformation("{Name}: {Amount} {Unit}", ingredient.Name, ingredient.Amount, ingredient.Unit);
        }
    }

    private async Task<Dictionary<Guid, ProductIngredients>> LoadIngredientsAsync(string ingredientsFilePath)
    {
        if (!File.Exists(ingredientsFilePath))
        {
            logger.LogError("Ingredients file not found at {IngredientsFile}", ingredientsFilePath);
            throw new FileNotFoundException($"Ingredients file not found at {ingredientsFilePath}");
        }

        var parser = parserFactory.GetParser<ProductIngredients>(ingredientsFilePath);
        var ingredients = await parser.ParseAsync(ingredientsFilePath);

        var ingredientsDictionary = ingredients.ToDictionary(i => i.ProductId, i => i);
        logger.LogInformation("Loaded {Count} ingredients from {IngredientsFile}.", ingredientsDictionary.Count, ingredientsFilePath);

        return ingredientsDictionary;
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