using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.Extensions.Logging;
using Pizzeria.App.Interfaces;
using Pizzeria.App.Models;
using Pizzeria.App.Services;

namespace Pizzeria.App.Tests.Services;

public partial class OrderServiceTests
{
    public readonly IFixture _fixture;
    public readonly IOrderParserFactory _mockParserFactory;
    public readonly IOrderValidator _mockOrderValidator;
    public readonly ILogger<OrderService> _mockLogger;
    public readonly IOrderParser<OrderItem> _mockParser;
    public readonly OrderService _sut;

    public OrderServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoNSubstituteCustomization());

        _mockParserFactory = _fixture.Freeze<IOrderParserFactory>();
        _mockOrderValidator = _fixture.Freeze<IOrderValidator>();
        _mockLogger = _fixture.Freeze<ILogger<OrderService>>();
        _mockParser = _fixture.Freeze<IOrderParser<OrderItem>>();

        _sut = new OrderService(_mockParserFactory, _mockOrderValidator, _mockLogger);
    }
    public static string GetTestDataPath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", fileName);
    }
}
