using AutoFixture;
using Pizzeria.App.Models;
using Pizzeria.App.Utils;
using Shouldly;

namespace Pizzeria.App.Tests.Utils;

public class OrderUtilsTests
{
    private readonly Fixture _fixture;

    public OrderUtilsTests()
    {
        _fixture = new Fixture();
    }

    private static OrderItem CreateOrderItem(Guid? orderId = null, Guid? productId = null, int quantity = 1)
        => new()
        {
            OrderId = orderId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity
        };

    private static Product CreateProduct(Guid? productId = null, string name = "Pizza", decimal price = 10.00m)
        => new()
        {
            ProductId = productId ?? Guid.NewGuid(),
            ProductName = name,
            Price = price
        };

    private static ProductIngredients CreateProductIngredients(Guid productId, params (string name, decimal amount, string unit)[] ingredients)
        => new()
        {
            ProductId = productId,
            Ingredients = ingredients.Select(i => new Ingredient(i.name, i.amount, i.unit)).ToList()
        };

    [Fact]
    public void CalculateOrderPrices_WhenValidOrdersWithProducts_ReturnsCorrectCalculations()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId, productId1, 2),
            CreateOrderItem(orderId, productId2, 1)
        };

        var products = new Dictionary<Guid, Product>
        {
            { productId1, CreateProduct(productId1, "Pizza1", 10.50m) },
            { productId2, CreateProduct(productId2, "Pizza2", 15.00m) }
        };

        // Act
        var result = OrderUtils.CalculateOrderPrices(orders, products);

        // Assert
        result.ShouldHaveSingleItem();
        var calculation = result[0];
        calculation.OrderId.ShouldBe(orderId);
        calculation.ItemCount.ShouldBe(2);
        calculation.TotalPrice.ShouldBe(36.00m); // (10.50 * 2) + (15.00 * 1)
        calculation.MissingProductIds.ShouldBeEmpty();
    }

    [Fact]
    public void CalculateOrderPrices_WhenMultipleOrders_ReturnsCalculationForEachOrder()
    {
        // Arrange
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId1, productId, 1),
            CreateOrderItem(orderId2, productId, 2)
        };

        var products = new Dictionary<Guid, Product>
        {
            { productId, CreateProduct(productId, "Pizza", 12.00m) }
        };

        // Act
        var result = OrderUtils.CalculateOrderPrices(orders, products);

        // Assert
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(2);
        result.Single(r => r.OrderId == orderId1).TotalPrice.ShouldBe(12.00m);
        result.Single(r => r.OrderId == orderId2).TotalPrice.ShouldBe(24.00m);
    }

    [Fact]
    public void CalculateOrderPrices_WhenMissingProducts_IncludesMissingProductIds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var existingProductId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId, existingProductId, 1),
            CreateOrderItem(orderId, missingProductId, 1)
        };

        var products = new Dictionary<Guid, Product>
        {
            { existingProductId, CreateProduct(existingProductId) }
        };

        // Act
        var result = OrderUtils.CalculateOrderPrices(orders, products);

        // Assert
        result.ShouldHaveSingleItem();
        var calculation = result[0];
        calculation.TotalPrice.ShouldBe(10.00m);
        calculation.MissingProductIds.ShouldContain(missingProductId);
        calculation.MissingProductIds.Count.ShouldBe(1);
    }

    [Fact]
    public void CalculateOrderPrices_WhenEmptyOrders_ReturnsEmptyList()
    {
        // Arrange
        var emptyOrders = new List<OrderItem>();
        var products = _fixture.CreateMany<Product>(2).ToDictionary(p => p.ProductId, p => p);

        // Act
        var result = OrderUtils.CalculateOrderPrices(emptyOrders, products);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void CalculateOrderPrices_WhenEmptyProducts_ReturnsZeroPriceWithAllMissingProducts()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId, productId, 2)
        };

        var emptyProducts = new Dictionary<Guid, Product>();

        // Act
        var result = OrderUtils.CalculateOrderPrices(orders, emptyProducts);

        // Assert
        result.ShouldHaveSingleItem();
        var calculation = result[0];
        calculation.TotalPrice.ShouldBe(0m);
        calculation.MissingProductIds.ShouldContain(productId);
    }

    [Fact]
    public void CalculateIngredientsAmount_WhenValidOrdersWithIngredients_ReturnsCorrectTotals()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId, productId1, 2),
            CreateOrderItem(orderId, productId2, 1)
        };

        var ingredients = new Dictionary<Guid, ProductIngredients>
        {
            { productId1, CreateProductIngredients(productId1, ("Tomato Sauce", 100, "ml"), ("Cheese", 150, "g")) },
            { productId2, CreateProductIngredients(productId2, ("Tomato Sauce", 50, "ml"), ("Pepperoni", 80, "g")) }
        };

        // Act
        var (totalIngredients, missingProducts) = OrderUtils.CalculateIngredientsAmount(orders, ingredients);

        // Assert
        totalIngredients.ShouldNotBeEmpty();
        totalIngredients.Count.ShouldBe(3);
        totalIngredients["Tomato Sauce_ml"].Amount.ShouldBe(250m); // (100 * 2) + (50 * 1)
        totalIngredients["Cheese_g"].Amount.ShouldBe(300m); // 150 * 2
        totalIngredients["Pepperoni_g"].Amount.ShouldBe(80m); // 80 * 1
        missingProducts.ShouldBeEmpty();
    }

    [Fact]
    public void CalculateIngredientsAmount_WhenMissingProductIngredients_TracksMissingProducts()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var existingProductId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId, existingProductId, 1),
            CreateOrderItem(orderId, missingProductId, 2)
        };

        var ingredients = new Dictionary<Guid, ProductIngredients>
        {
            { existingProductId, CreateProductIngredients(existingProductId, ("Flour", 200, "g")) }
        };

        // Act
        var (totalIngredients, missingProducts) = OrderUtils.CalculateIngredientsAmount(orders, ingredients);

        // Assert
        totalIngredients.Count.ShouldBe(1);
        totalIngredients["Flour_g"].Amount.ShouldBe(200m);
        missingProducts.ShouldNotBeEmpty();
        missingProducts.Count.ShouldBe(1);
        missingProducts[0].OrderId.ShouldBe(orderId);
        missingProducts[0].ProductId.ShouldBe(missingProductId);
    }

    [Fact]
    public void CalculateIngredientsAmount_WhenEmptyOrders_ReturnsEmptyResults()
    {
        // Arrange
        var emptyOrders = new List<OrderItem>();
        var ingredients = _fixture.CreateMany<ProductIngredients>(2).ToDictionary(p => p.ProductId, p => p);

        // Act
        var (totalIngredients, missingProducts) = OrderUtils.CalculateIngredientsAmount(emptyOrders, ingredients);

        // Assert
        totalIngredients.ShouldBeEmpty();
        missingProducts.ShouldBeEmpty();
    }

    [Fact]
    public void CalculateIngredientsAmount_WhenSameIngredientFromMultipleProducts_AggregatesCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var orders = new List<OrderItem>
        {
            CreateOrderItem(orderId, productId1, 1),
            CreateOrderItem(orderId, productId2, 1)
        };

        var ingredients = new Dictionary<Guid, ProductIngredients>
        {
            { productId1, CreateProductIngredients(productId1, ("Tomato Sauce", 100, "ml")) },
            { productId2, CreateProductIngredients(productId2, ("Tomato Sauce", 75, "ml")) }
        };

        // Act
        var (totalIngredients, missingProducts) = OrderUtils.CalculateIngredientsAmount(orders, ingredients);

        // Assert
        totalIngredients.ShouldHaveSingleItem();
        totalIngredients["Tomato Sauce_ml"].Amount.ShouldBe(175m); // 100 + 75
        missingProducts.ShouldBeEmpty();
    }
}
