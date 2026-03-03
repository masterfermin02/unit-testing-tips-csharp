using FluentAssertions;
using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Orders;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the Humble Object pattern.
///
/// When a class mixes complex logic with hard-to-test infrastructure (I/O, HTTP, DB),
/// extract the logic into a separate, easily testable "domain" class.
///
/// Humble Object = thin shell (controller, event handler) — hard to test, tested via integration tests
/// Extracted logic = pure domain service — easy to unit test
/// </summary>
public class HumbleObjectTests
{
    // ─────────────────────────────────────────────
    // EXTRACTED PURE LOGIC: OrderService
    // The controller (humble object) delegates to this
    // ─────────────────────────────────────────────

    [Fact]
    public void PlacingOrder_WithNoItems_ReturnsFailure()
    {
        var sut = new OrderService();
        var command = new PlaceOrderCommand(CustomerId.New(), Array.Empty<OrderItemDto>());

        var result = sut.Place(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("item");
    }

    [Fact]
    public void PlacingOrder_WithItemsTotalExceedingLimit_ReturnsFailure()
    {
        var sut = new OrderService();
        var command = new PlaceOrderCommand(
            CustomerId.New(),
            new[] { new OrderItemDto("ExpensiveItem", 15000m, 1) }
        );

        var result = sut.Place(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("maximum");
    }

    [Fact]
    public void PlacingOrder_WithValidItems_ReturnsSuccessWithId()
    {
        var sut = new OrderService();
        var command = new PlaceOrderCommand(
            CustomerId.New(),
            new[] { new OrderItemDto("Widget", 19.99m, 2) }
        );

        var result = sut.Place(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void PlacingOrder_AtExactMaximumTotal_Succeeds()
    {
        var sut = new OrderService();
        var command = new PlaceOrderCommand(
            CustomerId.New(),
            new[] { new OrderItemDto("Item", 10000m, 1) }
        );

        var result = sut.Place(command);

        result.IsSuccess.Should().BeTrue();
    }

    // The controller (humble object) is NOT unit tested here — that's fine.
    // It's tested via integration/API tests where the HTTP stack is involved.
    //
    // Key insight: by extracting the logic into OrderService,
    // we can unit test ALL business rules without spinning up HTTP/DB infrastructure.
}
