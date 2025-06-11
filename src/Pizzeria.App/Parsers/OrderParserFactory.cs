using System;
using Pizzeria.App.Interfaces;

namespace Pizzeria.App.Parsers;

public class OrderParserFactory : IOrderParserFactory
{
    public IOrderParser GetParser(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".json" => new JsonParser(),
            ".csv" => new CsvParser(),
            _ => throw new NotSupportedException($"File type '{extension}' is not supported.")
        };
    }
}
