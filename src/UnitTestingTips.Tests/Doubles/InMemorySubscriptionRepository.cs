using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Subscriptions;

namespace UnitTestingTips.Tests.Doubles;

public class InMemorySubscriptionRepository : ISubscriptionRepository
{
    private readonly Dictionary<Guid, (Subscription sub, CustomerId customerId)> _subscriptions = new();

    public void Save(Subscription subscription) =>
        _subscriptions[subscription.Id] = (subscription, new CustomerId(Guid.Empty));

    public void SaveForCustomer(Subscription subscription, CustomerId customerId) =>
        _subscriptions[subscription.Id] = (subscription, customerId);

    public Subscription? FindByCustomer(CustomerId customerId)
    {
        return _subscriptions.Values
            .Where(x => x.customerId == customerId)
            .Select(x => x.sub)
            .FirstOrDefault();
    }

    public int Count => _subscriptions.Count;
}
