namespace Pizzeria.App.Models;

public record OrderPriceCalculation(
    Guid OrderId,
    int ItemCount,
    decimal TotalPrice,
    List<Guid> MissingProductIds
);
