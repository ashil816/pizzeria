using AutoFixture;
using NSubstitute;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;
using Shouldly;

namespace Pizzeria.App.Tests.Services;

public partial class OrderServiceTests
{
    [Fact]
    public async Task CalculateOrderPriceAsync_WhenValidOrdersAndProducts_LogsPriceCalculations()
    {
        // Arrange
        var filePath = GetTestDataPath("products.json");
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            new() { OrderId = orderId, ProductId = productId1, Quantity = 2 },
            new() { OrderId = orderId, ProductId = productId2, Quantity = 1 }
        };

        var products = new List<Product>
        {
            new() { ProductId = productId1, ProductName = "Pizza1", Price = 10.50m },
            new() { ProductId = productId2, ProductName = "Pizza2", Price = 15.00m }
        };

        var mockProductParser = _fixture.Freeze<IOrderParser<Product>>();
        _mockParserFactory.GetParser<Product>(filePath).Returns(mockProductParser);
        mockProductParser.ParseAsync(filePath).Returns(products);

        // Act
        await _sut.CalculateOrderPriceAsync(orders, filePath);

        // Assert
        await mockProductParser.Received(1).ParseAsync(filePath);
        _mockParserFactory.Received(1).GetParser<Product>(filePath);
    }

    [Fact]
    public async Task CalculateOrderPriceAsync_WhenProductFileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var orders = _fixture.CreateMany<OrderItem>(2);
        var nonExistentFilePath = "/path/to/nonexistent/products.json";

        // Act & Assert
        var exception = await Should.ThrowAsync<FileNotFoundException>(() =>
            _sut.CalculateOrderPriceAsync(orders, nonExistentFilePath));

        exception.Message.ShouldContain("Products file not found");
    }

    [Fact]
    public async Task CalculateOrderPriceAsync_WhenParserThrowsException_PropagatesException()
    {
        // Arrange
        var filePath = GetTestDataPath("products.json");
        var orders = _fixture.CreateMany<OrderItem>(2);
        var parseException = new InvalidOperationException("Parser failed");

        var mockProductParser = _fixture.Freeze<IOrderParser<Product>>();
        _mockParserFactory.GetParser<Product>(filePath).Returns(mockProductParser);
        mockProductParser.ParseAsync(filePath).Returns(Task.FromException<IEnumerable<Product>>(parseException));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.CalculateOrderPriceAsync(orders, filePath));

        exception.ShouldBe(parseException);
    }

    [Fact]
    public async Task CalculateOrderPriceAsync_WhenEmptyOrders_ProcessesWithoutError()
    {
        // Arrange
        var filePath = GetTestDataPath("products.json");
        var emptyOrders = new List<OrderItem>();
        var products = _fixture.CreateMany<Product>(2);

        var mockProductParser = _fixture.Freeze<IOrderParser<Product>>();
        _mockParserFactory.GetParser<Product>(filePath).Returns(mockProductParser);
        mockProductParser.ParseAsync(filePath).Returns(products);

        // Act & Assert
        await Should.NotThrowAsync(() => _sut.CalculateOrderPriceAsync(emptyOrders, filePath));

        await mockProductParser.Received(1).ParseAsync(filePath);
    }

    [Fact]
    public async Task CalculateOrderPriceAsync_WhenMissingProducts_LogsWarnings()
    {
        // Arrange
        var filePath = GetTestDataPath("products.json");
        var existingProductId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            new() { OrderId = orderId, ProductId = existingProductId, Quantity = 1 },
            new() { OrderId = orderId, ProductId = missingProductId, Quantity = 1 }
        };

        var products = new List<Product>
        {
            new() { ProductId = existingProductId, ProductName = "Pizza1", Price = 10.00m }
        };

        var mockProductParser = _fixture.Freeze<IOrderParser<Product>>();
        _mockParserFactory.GetParser<Product>(filePath).Returns(mockProductParser);
        mockProductParser.ParseAsync(filePath).Returns(products);

        // Act
        await _sut.CalculateOrderPriceAsync(orders, filePath);

        // Assert
        await mockProductParser.Received(1).ParseAsync(filePath);
        _mockParserFactory.Received(1).GetParser<Product>(filePath);
    }
}
