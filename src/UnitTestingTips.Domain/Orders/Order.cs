using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Domain.Orders;

public class Order
{
    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    public DateTime CreatedAt { get; }
    public IReadOnlyList<OrderItem> Items { get; }
    public Money Total { get; }

    public Order(CustomerId customerId, DateTime createdAt, IEnumerable<OrderItem> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new ArgumentException("Order must have at least one item.", nameof(items));

        Id = OrderId.New();
        CustomerId = customerId;
        CreatedAt = createdAt;
        Items = itemList.AsReadOnly();
        Total = itemList.Aggregate(Money.Zero(), (sum, item) => sum + item.LineTotal);
    }

    public void ApplyDiscount(decimal discountPercent)
    {
        if (discountPercent <= 0)
            throw new ArgumentException("Discount must be greater than zero.", nameof(discountPercent));
        if (discountPercent > 100)
            throw new ArgumentException("Discount cannot exceed 100%.", nameof(discountPercent));
    }
}
