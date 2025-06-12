using Pizzeria.App.Models;

namespace Pizzeria.App.Utils;

public static class OrderUtils
{
    public static List<OrderPriceCalculation> CalculateOrderPrices(
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

    public static (Dictionary<string, Ingredient> totalIngredients, List<(Guid OrderId, Guid ProductId)> missingProducts)
        CalculateIngredientsAmount(IEnumerable<OrderItem> orders, Dictionary<Guid, ProductIngredients> ingredients)
    {
        var totalIngredients = new Dictionary<string, Ingredient>();
        var missingProducts = new List<(Guid OrderId, Guid ProductId)>();

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
                missingProducts.Add((order.OrderId, order.ProductId));
            }
        }

        return (totalIngredients, missingProducts);
    }
}
