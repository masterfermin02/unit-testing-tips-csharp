namespace UnitTestingTips.Domain.Customers;

public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public Customer Get(Guid id) => _repository.Get(id);

    public Customer? FindByEmail(string email) => _repository.FindByEmail(email);
}
