using Pizzeria.App.Models;

namespace Pizzeria.App.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderItem>> GetOrdersAsync(string filePath);
    Task CalculateOrderPriceAsync(IEnumerable<OrderItem> orders, string filePath);
    Task CalculateIngredientsAmountAsync(IEnumerable<OrderItem> orders, string filePath);
}
