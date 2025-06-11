using System;
using Pizzeria.App.Models;

namespace Pizzeria.App.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderItem>> GetOrdersAsync(string filePath);
}
