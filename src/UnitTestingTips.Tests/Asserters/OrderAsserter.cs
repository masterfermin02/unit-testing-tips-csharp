using FluentAssertions;
using UnitTestingTips.Domain.Orders;

namespace UnitTestingTips.Tests.Asserters;

public class OrderAsserter
{
    private readonly Order _order;

    private OrderAsserter(Order order)
    {
        _order = order;
    }

    public static OrderAsserter AssertThat(Order order) => new(order);

    public OrderAsserter WasCreatedAt(DateTime expectedDate)
    {
        _order.CreatedAt.Should().Be(expectedDate);
        return this;
    }

    public OrderAsserter HasTotal(decimal expectedTotal)
    {
        _order.Total.Amount.Should().Be(expectedTotal);
        return this;
    }

    public OrderAsserter HasItemCount(int expectedCount)
    {
        _order.Items.Should().HaveCount(expectedCount);
        return this;
    }

    public OrderAsserter ContainsItem(string name)
    {
        _order.Items.Should().Contain(i => i.Name == name,
            because: $"order should contain an item named '{name}'");
        return this;
    }
}
