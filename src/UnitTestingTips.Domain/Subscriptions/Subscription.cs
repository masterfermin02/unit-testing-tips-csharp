using UnitTestingTips.Domain.Common;

namespace UnitTestingTips.Domain.Subscriptions;

public class Subscription
{
    public Guid Id { get; }
    public SubscriptionStatus Status { get; private set; }
    public SubscriptionPlan? Plan { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public Subscription(SubscriptionStatus status, SubscriptionPlan? plan = null)
    {
        Id = Guid.NewGuid();
        Status = status;
        Plan = plan;
    }

    public void Activate(IClock clock)
    {
        if (Status == SubscriptionStatus.Active)
            throw new InvalidOperationException("Subscription is already active.");

        Status = SubscriptionStatus.Active;
        ExpiresAt = Plan is not null
            ? clock.UtcNow.AddMonths(Plan.DurationMonths)
            : clock.UtcNow.AddMonths(1);
    }

    public void Deactivate()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Only active subscriptions can be deactivated.");

        Status = SubscriptionStatus.Inactive;
    }

    public void Reactivate(IClock clock)
    {
        if (Status != SubscriptionStatus.Inactive)
            throw new InvalidOperationException("Only inactive subscriptions can be reactivated.");

        Status = SubscriptionStatus.Active;
        ExpiresAt = Plan is not null
            ? clock.UtcNow.AddMonths(Plan.DurationMonths)
            : clock.UtcNow.AddMonths(1);
    }

    public void RenewUntil(DateTime newExpiry)
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Only active subscriptions can be renewed.");

        ExpiresAt = newExpiry;
    }

    public bool IsActiveAt(DateTime date) =>
        Status == SubscriptionStatus.Active && ExpiresAt.HasValue && date <= ExpiresAt.Value;
}
