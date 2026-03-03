using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    void Save(Subscription subscription);
    Subscription? FindByCustomer(CustomerId customerId);
}
