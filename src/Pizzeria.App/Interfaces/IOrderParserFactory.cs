namespace Pizzeria.App.Interfaces;

public interface IOrderParserFactory
{
    IOrderParser<T> GetParser<T>(string filePath);
}
