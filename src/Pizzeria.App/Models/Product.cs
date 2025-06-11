namespace Pizzeria.App.Models;

public class Product
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public List<Ingredient> Ingredients { get; init; } = [];
}
