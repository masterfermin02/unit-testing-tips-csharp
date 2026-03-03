namespace UnitTestingTips.Domain.Auth;

public interface IUniqueEmailSpecification
{
    bool IsUnique(string email);
}
