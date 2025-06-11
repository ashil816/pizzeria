namespace Pizzeria.App.Interfaces;

public interface IOrderParserFactory
{
    IOrderParser GetParser(string filePath);
}
