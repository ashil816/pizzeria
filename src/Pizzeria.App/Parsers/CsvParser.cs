using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;

namespace Pizzeria.App.Parsers;

public class CsvParser : IOrderParser
{
    private readonly CsvConfiguration _config;

    public CsvParser()
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord  = true,
            TrimOptions      = TrimOptions.Trim,
            IgnoreBlankLines = true,
        };
    }
    public async Task<IEnumerable<OrderItem>> ParseAsync(string filePath)
    {
        var orders = new List<OrderItem>();
        await using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, _config);

        await foreach (var record in csv.GetRecordsAsync<OrderItem>())
        {
            orders.Add(record);
        }

        return orders;
    }
}
