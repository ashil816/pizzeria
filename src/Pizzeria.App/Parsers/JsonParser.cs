using System.Text.Json;
using Pizzeria.App.Interfaces;

namespace Pizzeria.App.Parsers;

public class JsonParser<T> : IOrderParser<T>
{
    public async Task<IEnumerable<T>> ParseAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var items = await JsonSerializer.DeserializeAsync<IEnumerable<T>>(stream);
        return items ?? [];
    }
}
