using Pizzeria.App.Models;

namespace Pizzeria.App.Interfaces;

public interface IOrderValidator
{
    Task<IEnumerable<OrderItem>> ValidateOrdersAsync(IEnumerable<OrderItem> orders);
}
