using UnitTestingTips.Tests.Asserters;
using UnitTestingTips.Tests.Builders;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the Assert Object (Asserter) pattern.
///
/// An Asserter wraps assertions in a fluent, domain-language API.
/// This makes complex assertions readable and reusable.
/// </summary>
public class AssertObjectTests
{
    [Fact]
    public void PlacingOrder_WithItems_CreatesOrderCorrectly()
    {
        var createdAt = new DateTime(2024, 6, 1);

        var order = new OrderBuilder()
            .CreatedAt(createdAt)
            .WithItem("Widget", 10.00m)
            .WithItem("Gadget", 20.00m)
            .Build();

        // Fluent, domain-language assertions — reads like a specification
        OrderAsserter.AssertThat(order)
            .WasCreatedAt(createdAt)
            .HasTotal(30.00m)
            .HasItemCount(2)
            .ContainsItem("Widget")
            .ContainsItem("Gadget");
    }

    [Fact]
    public void PlacingOrder_WithQuantity_HasCorrectTotal()
    {
        var order = new OrderBuilder()
            .WithItem("Widget", 10.00m, quantity: 3)
            .Build();

        OrderAsserter.AssertThat(order)
            .HasTotal(30.00m)
            .HasItemCount(1)
            .ContainsItem("Widget");
    }
}
