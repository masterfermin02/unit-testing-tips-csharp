using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Orders;
using UnitTestingTips.Tests.Builders;

namespace UnitTestingTips.Tests.Mothers;

public static class OrderMother
{
    public static Order Simple() =>
        new OrderBuilder()
            .WithItem("Widget", 19.99m)
            .Build();

    public static Order WithMultipleItems() =>
        new OrderBuilder()
            .WithItem("Widget", 19.99m)
            .WithItem("Gadget", 49.99m)
            .WithItem("Doohickey", 9.99m)
            .Build();

    public static Order ForCustomer(CustomerId customerId) =>
        new OrderBuilder()
            .ForCustomer(customerId)
            .WithItem("Widget", 19.99m)
            .Build();
}
