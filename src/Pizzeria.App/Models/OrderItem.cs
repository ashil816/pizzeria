using System;

namespace Pizzeria.App.Models;

public class OrderItem
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime DeliveryAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string DeliveryAddress { get; init; } = string.Empty;
}
