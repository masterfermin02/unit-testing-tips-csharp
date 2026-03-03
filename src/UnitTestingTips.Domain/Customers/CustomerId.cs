namespace UnitTestingTips.Domain.Customers;

public record CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
