using AutoFixture;
using NSubstitute;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;
using Shouldly;

namespace Pizzeria.App.Tests.Services;

public partial class OrderServiceTests
{
    [Fact]
    public async Task CalculateIngredientsAmountAsync_WhenValidOrdersAndIngredients_LogsIngredientCalculations()
    {
        // Arrange
        var filePath = GetTestDataPath("ingredients.json");
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            new() { OrderId = orderId, ProductId = productId1, Quantity = 2 },
            new() { OrderId = orderId, ProductId = productId2, Quantity = 1 }
        };

        var ingredients = new List<ProductIngredients>
        {
            new()
            {
                ProductId = productId1,
                Ingredients =
                [
                    new("Tomato Sauce", 150, "ml"),
                    new("Mozzarella Cheese", 200, "g")
                ]
            },
            new()
            {
                ProductId = productId2,
                Ingredients =
                [
                    new("Tomato Sauce", 100, "ml"),
                    new("Pepperoni", 80, "g")
                ]
            }
        };

        var mockIngredientsParser = _fixture.Freeze<IOrderParser<ProductIngredients>>();
        _mockParserFactory.GetParser<ProductIngredients>(filePath).Returns(mockIngredientsParser);
        mockIngredientsParser.ParseAsync(filePath).Returns(ingredients);

        // Act
        await _sut.CalculateIngredientsAmountAsync(orders, filePath);

        // Assert
        await mockIngredientsParser.Received(1).ParseAsync(filePath);
        _mockParserFactory.Received(1).GetParser<ProductIngredients>(filePath);
    }

    [Fact]
    public async Task CalculateIngredientsAmountAsync_WhenIngredientsFileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var orders = _fixture.CreateMany<OrderItem>(2);
        var nonExistentFilePath = "/path/to/nonexistent/ingredients.json";

        // Act & Assert
        var exception = await Should.ThrowAsync<FileNotFoundException>(() =>
            _sut.CalculateIngredientsAmountAsync(orders, nonExistentFilePath));

        exception.Message.ShouldContain("Ingredients file not found");
    }

    [Fact]
    public async Task CalculateIngredientsAmountAsync_WhenParserThrowsException_PropagatesException()
    {
        // Arrange
        var filePath = GetTestDataPath("ingredients.json");
        var orders = _fixture.CreateMany<OrderItem>(2);
        var parseException = new InvalidOperationException("Parser failed");

        var mockIngredientsParser = _fixture.Freeze<IOrderParser<ProductIngredients>>();
        _mockParserFactory.GetParser<ProductIngredients>(filePath).Returns(mockIngredientsParser);
        mockIngredientsParser.ParseAsync(filePath).Returns(Task.FromException<IEnumerable<ProductIngredients>>(parseException));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.CalculateIngredientsAmountAsync(orders, filePath));

        exception.ShouldBe(parseException);
    }

    [Fact]
    public async Task CalculateIngredientsAmountAsync_WhenEmptyOrders_ProcessesWithoutError()
    {
        // Arrange
        var filePath = GetTestDataPath("ingredients.json");
        var emptyOrders = new List<OrderItem>();
        var ingredients = _fixture.CreateMany<ProductIngredients>(2);

        var mockIngredientsParser = _fixture.Freeze<IOrderParser<ProductIngredients>>();
        _mockParserFactory.GetParser<ProductIngredients>(filePath).Returns(mockIngredientsParser);
        mockIngredientsParser.ParseAsync(filePath).Returns(ingredients);

        // Act & Assert
        await Should.NotThrowAsync(() => _sut.CalculateIngredientsAmountAsync(emptyOrders, filePath));

        await mockIngredientsParser.Received(1).ParseAsync(filePath);
    }

    [Fact]
    public async Task CalculateIngredientsAmountAsync_WhenMissingProductIngredients_LogsWarnings()
    {
        // Arrange
        var filePath = GetTestDataPath("ingredients.json");
        var existingProductId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            new() { OrderId = orderId, ProductId = existingProductId, Quantity = 1 },
            new() { OrderId = orderId, ProductId = missingProductId, Quantity = 1 }
        };

        var ingredients = new List<ProductIngredients>
        {
            new()
            {
                ProductId = existingProductId,
                Ingredients = [new("Tomato Sauce", 150, "ml")]
            }
        };

        var mockIngredientsParser = _fixture.Freeze<IOrderParser<ProductIngredients>>();
        _mockParserFactory.GetParser<ProductIngredients>(filePath).Returns(mockIngredientsParser);
        mockIngredientsParser.ParseAsync(filePath).Returns(ingredients);

        // Act
        await _sut.CalculateIngredientsAmountAsync(orders, filePath);

        // Assert
        await mockIngredientsParser.Received(1).ParseAsync(filePath);
        _mockParserFactory.Received(1).GetParser<ProductIngredients>(filePath);
    }
}
