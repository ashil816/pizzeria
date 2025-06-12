using Pizzeria.App.Interfaces;

namespace Pizzeria.App.Models;

public class Product : IHasProductId
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
