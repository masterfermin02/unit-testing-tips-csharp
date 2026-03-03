namespace UnitTestingTips.Domain.Orders;

public class OrderItem
{
    public string Name { get; }
    public Money UnitPrice { get; }
    public int Quantity { get; }
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    public OrderItem(string name, Money unitPrice, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name cannot be empty.", nameof(name));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        Name = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
