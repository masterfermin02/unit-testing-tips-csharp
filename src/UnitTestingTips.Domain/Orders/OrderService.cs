namespace UnitTestingTips.Domain.Orders;

public class OrderService
{
    private const decimal MaxOrderTotal = 10000m;

    public Result<OrderId> Place(PlaceOrderCommand command)
    {
        if (!command.Items.Any())
            return Result.Failure<OrderId>("Order must have at least one item.");

        var total = command.Items.Sum(i => i.Price * i.Quantity);
        if (total > MaxOrderTotal)
            return Result.Failure<OrderId>($"Order total exceeds the maximum allowed of {MaxOrderTotal:C}.");

        var items = command.Items
            .Select(i => new OrderItem(i.Name, new Money(i.Price), i.Quantity))
            .ToList();

        var order = new Order(command.CustomerId, DateTime.UtcNow, items);
        return Result.Success(order.Id);
    }
}
