using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using Pizzeria.App.Models;
using Shouldly;

namespace Pizzeria.App.Tests.Services;

public partial class OrderServiceTests
{
    [Fact]
    public async Task GetOrdersAsync_WhenValidFileWithMultipleOrders_ReturnsValidatedOrders()
    {
        // Arrange
        var filePath = GetTestDataPath("valid-orders.json");
        var parsedOrders = _fixture.CreateMany<OrderItem>(3).ToList();
        var validatedOrders = _fixture.CreateMany<OrderItem>(2).ToList();

        _mockParserFactory.GetParser<OrderItem>(filePath).Returns(_mockParser);
        _mockParser.ParseAsync(filePath).Returns(parsedOrders);
        _mockOrderValidator.ValidateOrdersAsync(parsedOrders).Returns(validatedOrders);

        // Act
        var result = await _sut.GetOrdersAsync(filePath);

        // Assert
        result.ShouldBe(validatedOrders);
        await _mockParser.Received(1).ParseAsync(filePath);
        await _mockOrderValidator.Received(1).ValidateOrdersAsync(parsedOrders);
        _mockParserFactory.Received(1).GetParser<OrderItem>(filePath);
    }

    [Fact]
    public async Task GetOrdersAsync_WhenFileDoesNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentFilePath = "/path/to/nonexistent/file.json";

        // Act
        var result = await _sut.GetOrdersAsync(nonExistentFilePath);

        // Assert
        result.ShouldBeEmpty();
        _mockParserFactory.DidNotReceive().GetParser<OrderItem>(Arg.Any<string>());
        await _mockParser.DidNotReceive().ParseAsync(Arg.Any<string>());
        await _mockOrderValidator.DidNotReceive().ValidateOrdersAsync(Arg.Any<IEnumerable<OrderItem>>());
    }

    [Fact]
    public async Task GetOrdersAsync_WhenValidFileButValidationFiltersOutSomeOrders_ReturnsOnlyValidOrders()
    {
        // Arrange
        var filePath = GetTestDataPath("valid-orders.json");
        var parsedOrders = _fixture.CreateMany<OrderItem>(5).ToList();
        var validOrder = _fixture.Create<OrderItem>();
        var validatedOrders = new List<OrderItem> { validOrder };

        _mockParserFactory.GetParser<OrderItem>(filePath).Returns(_mockParser);
        _mockParser.ParseAsync(filePath).Returns(parsedOrders);
        _mockOrderValidator.ValidateOrdersAsync(parsedOrders).Returns(validatedOrders);

        // Act
        var result = await _sut.GetOrdersAsync(filePath);

        // Assert
        result.ShouldHaveSingleItem();
        result.Single().ShouldBe(validOrder);
        result.Count().ShouldBeLessThan(parsedOrders.Count);
        await _mockParser.Received(1).ParseAsync(filePath);
        await _mockOrderValidator.Received(1).ValidateOrdersAsync(parsedOrders);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/completely/fake/path.json")]
    public async Task GetOrdersAsync_WhenInvalidFilePath_ReturnsEmptyCollection(
        string invalidFilePath)
    {
        // Arrange & Act
        var result = await _sut.GetOrdersAsync(invalidFilePath);

        // Assert
        result.ShouldBeEmpty();
        _mockParserFactory.DidNotReceive().GetParser<OrderItem>(Arg.Any<string>());
        await _mockParser.DidNotReceive().ParseAsync(Arg.Any<string>());
        await _mockOrderValidator.DidNotReceive().ValidateOrdersAsync(Arg.Any<IEnumerable<OrderItem>>());
    }

    [Theory, AutoData]
    public async Task GetOrdersAsync_WhenParserThrowsException_PropagatesException(
        Exception parseException)
    {
        // Arrange
        var filePath = GetTestDataPath("valid-orders.json");
        _mockParserFactory.GetParser<OrderItem>(filePath).Returns(_mockParser);
        _mockParser.ParseAsync(filePath).Returns(Task.FromException<IEnumerable<OrderItem>>(parseException));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _sut.GetOrdersAsync(filePath));
        exception.ShouldBe(parseException);

        await _mockParser.Received(1).ParseAsync(filePath);
        await _mockOrderValidator.DidNotReceive().ValidateOrdersAsync(Arg.Any<IEnumerable<OrderItem>>());
    }

    [Fact]
    public async Task GetOrdersAsync_WhenLargeFile_ProcessesAllOrders()
    {
        // Arrange
        var filePath = GetTestDataPath("valid-orders.json");
        var largeOrderSet = _fixture.CreateMany<OrderItem>(1000).ToList();

        _mockParserFactory.GetParser<OrderItem>(filePath).Returns(_mockParser);
        _mockParser.ParseAsync(filePath).Returns(largeOrderSet);
        _mockOrderValidator.ValidateOrdersAsync(largeOrderSet).Returns(largeOrderSet);

        // Act
        var result = await _sut.GetOrdersAsync(filePath);

        // Assert
        result.Count().ShouldBe(1000);
        await _mockParser.Received(1).ParseAsync(filePath);
        await _mockOrderValidator.Received(1).ValidateOrdersAsync(largeOrderSet);
    }

    [Fact]
    public async Task GetOrdersAsync_WhenJsonFile_CallsCorrectParser()
    {
        // Arrange
        var jsonFilePath = GetTestDataPath("valid-orders.json");
        var orders = _fixture.CreateMany<OrderItem>(2).ToList();

        _mockParserFactory.GetParser<OrderItem>(jsonFilePath).Returns(_mockParser);
        _mockParser.ParseAsync(jsonFilePath).Returns(orders);
        _mockOrderValidator.ValidateOrdersAsync(orders).Returns(orders);

        // Act
        await _sut.GetOrdersAsync(jsonFilePath);

        // Assert
        _mockParserFactory.Received(1).GetParser<OrderItem>(jsonFilePath);
    }

    [Fact]
    public async Task GetOrdersAsync_WhenCsvFile_CallsCorrectParser()
    {
        // Arrange
        var csvFilePath = GetTestDataPath("valid-orders.csv");
        var orders = _fixture.CreateMany<OrderItem>(2).ToList();

        _mockParserFactory.GetParser<OrderItem>(csvFilePath).Returns(_mockParser);
        _mockParser.ParseAsync(csvFilePath).Returns(orders);
        _mockOrderValidator.ValidateOrdersAsync(orders).Returns(orders);

        // Act
        await _sut.GetOrdersAsync(csvFilePath);

        // Assert
        _mockParserFactory.Received(1).GetParser<OrderItem>(csvFilePath);
    }

    [Fact]
    public async Task GetOrdersAsync_WhenAllOrdersFilteredOut_ReturnsEmptyCollection()
    {
        // Arrange
        var filePath = GetTestDataPath("valid-orders.json");
        var parsedOrders = _fixture.CreateMany<OrderItem>(5).ToList();
        var emptyValidatedOrders = new List<OrderItem>();

        _mockParserFactory.GetParser<OrderItem>(filePath).Returns(_mockParser);
        _mockParser.ParseAsync(filePath).Returns(parsedOrders);
        _mockOrderValidator.ValidateOrdersAsync(parsedOrders).Returns(emptyValidatedOrders);

        // Act
        var result = await _sut.GetOrdersAsync(filePath);

        // Assert
        result.ShouldBeEmpty();
        await _mockParser.Received(1).ParseAsync(filePath);
        await _mockOrderValidator.Received(1).ValidateOrdersAsync(parsedOrders);
    }
}
