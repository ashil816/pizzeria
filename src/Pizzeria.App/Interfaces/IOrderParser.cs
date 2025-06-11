using System;

namespace Pizzeria.App.Interfaces;

public interface IOrderParser<T>
{
    Task<IEnumerable<T>> ParseAsync(string filePath);
}
