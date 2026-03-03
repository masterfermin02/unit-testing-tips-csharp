using UnitTestingTips.Domain.Auth;

namespace UnitTestingTips.Tests.Doubles;

public class AlwaysUniqueEmailStub : IUniqueEmailSpecification
{
    public bool IsUnique(string email) => true;
}

public class NeverUniqueEmailStub : IUniqueEmailSpecification
{
    public bool IsUnique(string email) => false;
}
