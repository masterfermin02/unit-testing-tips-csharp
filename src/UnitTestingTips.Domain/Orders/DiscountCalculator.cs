using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Domain.Orders;

public class DiscountCalculator
{
    public decimal Calculate(CustomerTier tier, decimal orderAmount)
    {
        return tier switch
        {
            CustomerTier.Premium => orderAmount * 0.20m,
            CustomerTier.Enterprise => orderAmount * 0.30m,
            CustomerTier.Standard when orderAmount >= 100m => orderAmount * 0.05m,
            _ => 0m
        };
    }
}
