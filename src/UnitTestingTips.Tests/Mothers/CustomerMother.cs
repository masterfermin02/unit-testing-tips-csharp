using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Tests.Mothers;

public static class CustomerMother
{
    public static Customer Active() =>
        new(CustomerId.New(), "Alice Smith", "alice@example.com", CustomerTier.Standard);

    public static Customer Premium() =>
        new(CustomerId.New(), "Bob Jones", "bob@example.com", CustomerTier.Premium);

    public static Customer WithEmail(string email) =>
        new(CustomerId.New(), "Test User", email, CustomerTier.Standard);
}
