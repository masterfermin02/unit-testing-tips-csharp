using FluentAssertions;
using UnitTestingTips.Tests.Builders;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the Builder pattern for test object construction.
///
/// Use Builder when:
/// - An object has many varying properties
/// - Different tests need different combinations of properties
/// - You want a fluent, readable test setup
///
/// Use Object Mother when:
/// - You have a small set of well-known named states
/// </summary>
public class BuilderTests
{
    [Fact]
    public void CalculatingTotal_WithSingleItem_ReturnsItemPrice()
    {
        var order = new OrderBuilder()
            .WithItem("Widget", 19.99m)
            .Build();

        order.Total.Amount.Should().Be(19.99m);
    }

    [Fact]
    public void CalculatingTotal_WithMultipleItems_SumsAllLineTotals()
    {
        var order = new OrderBuilder()
            .WithItem("Widget", 10.00m)
            .WithItem("Gadget", 20.00m)
            .WithItem("Doohickey", 5.00m)
            .Build();

        order.Total.Amount.Should().Be(35.00m);
    }

    [Fact]
    public void CalculatingTotal_WithQuantity_MultipliesUnitPrice()
    {
        var order = new OrderBuilder()
            .WithItem("Widget", 19.99m, quantity: 3)
            .Build();

        order.Total.Amount.Should().Be(59.97m);
    }

    [Fact]
    public void Order_CreatedAt_IsPreserved()
    {
        var createdAt = new DateTime(2024, 6, 15);

        var order = new OrderBuilder()
            .CreatedAt(createdAt)
            .WithItem("Widget", 10m)
            .Build();

        order.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Building_OrderWithMixedItems_CalculatesCorrectTotal()
    {
        // Fluent builder reads like a specification
        var order = new OrderBuilder()
            .CreatedAt(new DateTime(2024, 1, 15))
            .WithItem("Widget", 19.99m)
            .WithItem("Gadget", 49.99m, quantity: 2)
            .Build();

        order.Total.Amount.Should().Be(119.97m);
    }
}
