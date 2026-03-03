namespace UnitTestingTips.Domain.Orders;

public record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero() => new(0m);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add amounts in different currencies.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    public override string ToString() => $"{Amount:F2} {Currency}";
}
