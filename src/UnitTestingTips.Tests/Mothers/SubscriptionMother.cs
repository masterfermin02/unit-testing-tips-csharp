using UnitTestingTips.Domain.Common;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Doubles;

namespace UnitTestingTips.Tests.Mothers;

public static class SubscriptionMother
{
    public static Subscription New() =>
        new(SubscriptionStatus.New);

    public static Subscription Active()
    {
        var sub = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());
        sub.Activate(new FixedClock(DateTime.UtcNow));
        return sub;
    }

    public static Subscription Inactive()
    {
        var sub = Active();
        sub.Deactivate();
        return sub;
    }

    public static Subscription ActiveWithPlan(SubscriptionPlan plan)
    {
        var sub = new Subscription(SubscriptionStatus.New, plan);
        sub.Activate(new FixedClock(DateTime.UtcNow));
        return sub;
    }
}
