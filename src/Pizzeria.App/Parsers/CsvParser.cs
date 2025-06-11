using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Pizzeria.App.Interfaces;

namespace Pizzeria.App.Parsers;

public class CsvParser<T> : IOrderParser<T>
{
    private readonly CsvConfiguration _config;

    public CsvParser()
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
        };
    }
    public async Task<IEnumerable<T>> ParseAsync(string filePath)
    {
        var items = new List<T>();
        await using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, _config);

        await foreach (var record in csv.GetRecordsAsync<T>())
        {
            items.Add(record);
        }

        return items;
    }
}
