namespace UnitTestingTips.Domain.Subscriptions;

public class SubscriptionPlan
{
    public string Name { get; }
    public decimal MonthlyPrice { get; }
    public int DurationMonths { get; }

    public SubscriptionPlan(string name, decimal monthlyPrice, int durationMonths = 1)
    {
        Name = name;
        MonthlyPrice = monthlyPrice;
        DurationMonths = durationMonths;
    }

    public static SubscriptionPlan Monthly() => new("Monthly", 9.99m, 1);
    public static SubscriptionPlan Annual() => new("Annual", 7.99m, 12);
}
