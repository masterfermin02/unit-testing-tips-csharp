namespace UnitTestingTips.Domain.Customers;

public class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException(Guid id)
        : base($"Customer with id '{id}' was not found.") { }

    public CustomerNotFoundException(string email)
        : base($"Customer with email '{email}' was not found.") { }
}
