namespace UnitTestingTips.Domain.Customers;

public class Customer
{
    public CustomerId Id { get; }
    public string Name { get; }
    public string Email { get; }
    public CustomerTier Tier { get; private set; }
    public bool IsActive { get; private set; }

    public Customer(CustomerId id, string name, string email, CustomerTier tier = CustomerTier.Standard)
    {
        Id = id;
        Name = name;
        Email = email;
        Tier = tier;
        IsActive = true;
    }

    public void Upgrade(CustomerTier newTier)
    {
        if (newTier <= Tier)
            throw new InvalidOperationException("Can only upgrade to a higher tier.");
        Tier = newTier;
    }

    public void Deactivate() => IsActive = false;
}
