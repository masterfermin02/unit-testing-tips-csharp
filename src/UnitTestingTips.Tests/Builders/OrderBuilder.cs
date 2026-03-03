using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Orders;

namespace UnitTestingTips.Tests.Builders;

public class OrderBuilder
{
    private DateTime _createdAt = new DateTime(2024, 1, 1);
    private readonly List<OrderItem> _items = new();
    private CustomerId _customerId = CustomerId.New();

    public OrderBuilder CreatedAt(DateTime date)
    {
        _createdAt = date;
        return this;
    }

    public OrderBuilder ForCustomer(CustomerId customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithItem(string name, decimal price, int quantity = 1)
    {
        _items.Add(new OrderItem(name, new Money(price), quantity));
        return this;
    }

    public Order Build()
    {
        if (_items.Count == 0)
            WithItem("Default Item", 10m);

        return new Order(_customerId, _createdAt, _items);
    }
}
