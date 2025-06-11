using System;
using Pizzeria.App.Models;

namespace Pizzeria.App.Interfaces;

public interface IOrderParser
{
    Task<IEnumerable<OrderItem>> ParseAsync(string filePath);
}
