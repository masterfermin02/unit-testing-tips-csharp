using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Domain.Orders;

public record PlaceOrderCommand(CustomerId CustomerId, IReadOnlyList<OrderItemDto> Items);

public record OrderItemDto(string Name, decimal Price, int Quantity = 1);
