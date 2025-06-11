using System;

namespace Pizzeria.App.Models;

public class Ingredient
{
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Unit { get; init; } = string.Empty;
}
