using UnitTestingTips.Domain.Customers;

namespace UnitTestingTips.Tests.Doubles;

public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _customers = new();

    public void Store(Customer customer)
    {
        _customers[customer.Id.Value] = customer;
    }

    public Customer Get(Guid id)
    {
        if (!_customers.TryGetValue(id, out var customer))
            throw new CustomerNotFoundException(id);

        return customer;
    }

    public Customer? FindByEmail(string email)
    {
        return _customers.Values.FirstOrDefault(c => c.Email == email);
    }
}
