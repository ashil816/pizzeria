using Pizzeria.App.Interfaces;

namespace Pizzeria.App.Models;

public class ProductIngredients : IHasProductId
{
    public Guid ProductId { get; init; }
    public List<Ingredient> Ingredients { get; init; } = [];
}
