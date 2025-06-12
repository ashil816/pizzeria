using Pizzeria.App.Models;
using Pizzeria.App.Validators;
using Shouldly;

namespace Pizzeria.App.Tests.Validators;

public class OrderValidatorTests
{
    private readonly OrderValidator _sut;

    public OrderValidatorTests()
    {
        _sut = new OrderValidator();
    }

    private static OrderItem CreateValidOrderItem(
        Guid? orderId = null,
        Guid? productId = null,
        int quantity = 2,
        DateTime? createdAt = null,
        DateTime? deliveryAt = null,
        string? deliveryAddress = "123 Main St")
    {
        var created = createdAt ?? DateTime.Now.AddHours(-1);
        var delivery = deliveryAt ?? DateTime.Now.AddHours(1);

        return new OrderItem
        {
            OrderId = orderId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity,
            CreatedAt = created,
            DeliveryAt = delivery,
            DeliveryAddress = deliveryAddress ?? "123 Main St"
        };
    }

    [Fact]
    public async Task ValidateOrdersAsync_WhenAllOrdersValid_ReturnsAllOrders()
    {
        // Arrange
        var validOrders = new List<OrderItem>
        {
            CreateValidOrderItem(quantity: 1),
            CreateValidOrderItem(quantity: 25),
            CreateValidOrderItem(quantity: 10)
        };

        // Act
        var result = await _sut.ValidateOrdersAsync(validOrders);

        // Assert
        result.Count().ShouldBe(3);
        result.ShouldBe(validOrders);
    }

    [Fact]
    public async Task ValidateOrdersAsync_WhenEmptyGuidOrderId_FiltersOutInvalidOrder()
    {
        // Arrange
        var orders = new List<OrderItem>
        {
            CreateValidOrderItem(),
            CreateValidOrderItem(orderId: Guid.Empty)
        };

        // Act
        var result = await _sut.ValidateOrdersAsync(orders);

        // Assert
        result.ShouldHaveSingleItem();
        result.First().OrderId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task ValidateOrdersAsync_WhenEmptyGuidProductId_FiltersOutInvalidOrder()
    {
        // Arrange
        var orders = new List<OrderItem>
        {
            CreateValidOrderItem(),
            CreateValidOrderItem(productId: Guid.Empty)
        };

        // Act
        var result = await _sut.ValidateOrdersAsync(orders);

        // Assert
        result.ShouldHaveSingleItem();
        result.First().ProductId.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(26)]
    [InlineData(100)]
    public async Task ValidateOrdersAsync_WhenInvalidQuantity_FiltersOutInvalidOrder(int invalidQuantity)
    {
        // Arrange
        var orders = new List<OrderItem>
        {
            CreateValidOrderItem(quantity: 5),
            CreateValidOrderItem(quantity: invalidQuantity)
        };

        // Act
        var result = await _sut.ValidateOrdersAsync(orders);

        // Assert
        result.ShouldHaveSingleItem();
        result.First().Quantity.ShouldBe(5);
    }

    [Fact]
    public async Task ValidateOrdersAsync_WhenDeliveryAtBeforeCreatedAt_FiltersOutInvalidOrder()
    {
        // Arrange
        var baseTime = DateTime.Now;
        var orders = new List<OrderItem>
        {
            CreateValidOrderItem(createdAt: baseTime, deliveryAt: baseTime.AddHours(1)),
            CreateValidOrderItem(createdAt: baseTime, deliveryAt: baseTime.AddHours(-1))
        };

        // Act
        var result = await _sut.ValidateOrdersAsync(orders);

        // Assert
        result.ShouldHaveSingleItem();
        result.First().DeliveryAt.ShouldBeGreaterThan(result.First().CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateOrdersAsync_WhenInvalidDeliveryAddress_FiltersOutInvalidOrder(string? invalidAddress)
    {
        // Arrange
        var validOrder = CreateValidOrderItem(deliveryAddress: "Valid Address");
        var invalidOrder = new OrderItem
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 5,
            CreatedAt = DateTime.Now.AddHours(-1),
            DeliveryAt = DateTime.Now.AddHours(1),
            DeliveryAddress = invalidAddress!
        };

        var orders = new List<OrderItem> { validOrder, invalidOrder };

        // Act
        var result = await _sut.ValidateOrdersAsync(orders);

        // Assert
        result.ShouldHaveSingleItem();
        result.First().DeliveryAddress.ShouldBe("Valid Address");
    }

    [Fact]
    public async Task ValidateOrdersAsync_WhenEmptyInput_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyOrders = new List<OrderItem>();

        // Act
        var result = await _sut.ValidateOrdersAsync(emptyOrders);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateOrdersAsync_WhenAllOrdersInvalid_ReturnsEmptyCollection()
    {
        // Arrange
        var invalidOrders = new List<OrderItem>
        {
            CreateValidOrderItem(orderId: Guid.Empty),
            CreateValidOrderItem(quantity: 0),
            CreateValidOrderItem(deliveryAddress: "")
        };

        // Act
        var result = await _sut.ValidateOrdersAsync(invalidOrders);

        // Assert
        result.ShouldBeEmpty();
    }
}
