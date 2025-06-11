using System.Text.Json;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;

namespace Pizzeria.App.Parsers;

public class JsonParser : IOrderParser
{
    public async Task<IEnumerable<OrderItem>> ParseAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var orderItems = await JsonSerializer.DeserializeAsync<IEnumerable<OrderItem>>(stream);
        return orderItems ?? [];
    }
}
