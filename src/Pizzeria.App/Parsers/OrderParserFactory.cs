using System;
using Pizzeria.App.Interfaces;

namespace Pizzeria.App.Parsers;

public class OrderParserFactory : IOrderParserFactory
{
    public IOrderParser<T> GetParser<T>(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".json" => new JsonParser<T>(),
            ".csv" => new CsvParser<T>(),
            _ => throw new NotSupportedException($"File type '{extension}' is not supported.")
        };
    }
}
