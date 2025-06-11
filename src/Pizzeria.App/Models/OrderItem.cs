using System;

namespace Pizzeria.App.Models;

public class OrderItem
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime DeliveryAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string DeliveryAddress { get; init; } = string.Empty;
}
