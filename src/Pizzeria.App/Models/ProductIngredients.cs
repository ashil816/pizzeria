using System;

namespace Pizzeria.App.Models;

public class ProductIngredients
{
    public Guid ProductId { get; init; }
    public List<Ingredient> Ingredients { get; init; } = [];
}
