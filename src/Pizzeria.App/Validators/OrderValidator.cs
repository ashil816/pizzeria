using System;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;

namespace Pizzeria.App.Validators;

public class OrderValidator : IOrderValidator
{
    public Task<IEnumerable<OrderItem>> ValidateOrdersAsync(IEnumerable<OrderItem> orders)
    {
        var validOrders = new List<OrderItem>();

        foreach (var order in orders)
        {
            var isValid = true;

            if (order.OrderId == Guid.Empty)
            {
                isValid = false;
            }

            if (order.ProductId == Guid.Empty)
            {
                isValid = false;
            }

            if (order.Quantity < 1 || order.Quantity > 25)
            {
                isValid = false;
            }

            if (order.DeliveryAt <= order.CreatedAt)
            {
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
            {
                isValid = false;
            }

            if (isValid) validOrders.Add(order);
        }

        return Task.FromResult<IEnumerable<OrderItem>>(validOrders);
    }
}
