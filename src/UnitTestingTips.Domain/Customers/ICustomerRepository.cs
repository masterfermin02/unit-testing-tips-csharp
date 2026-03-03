namespace UnitTestingTips.Domain.Customers;

public interface ICustomerRepository
{
    void Store(Customer customer);
    Customer Get(Guid id);
    Customer? FindByEmail(string email);
}
