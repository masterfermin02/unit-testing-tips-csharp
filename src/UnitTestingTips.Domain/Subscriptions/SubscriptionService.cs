using UnitTestingTips.Domain.Common;
using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Domain.Subscriptions;

public class SubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IClock _clock;

    public SubscriptionService(ISubscriptionRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Subscription Purchase(Customer customer, SubscriptionPlan plan)
    {
        var subscription = new Subscription(SubscriptionStatus.New, plan);
        subscription.Activate(_clock);
        _repository.Save(subscription);
        return subscription;
    }

    public void Renew(Subscription subscription)
    {
        subscription.RenewUntil(_clock.UtcNow.AddMonths(subscription.Plan?.DurationMonths ?? 1));
    }
}
