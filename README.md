# Testing tips

In these times, the benefits of writing unit tests are huge. I think that most of the recently started projects contain any unit tests. In enterprise applications with a lot of business logic, unit tests are the most important tests, because they are fast and can us instantly assure that our implementation is correct. However, I often see a problem with good tests in projects, though these tests' benefits are only huge when you have good unit tests. So in these examples, I will try to share some tips on what to do to write good unit tests.

## Unit Test Doubles and Naming Guidelines in C#

### Test Doubles

---

### Dummy

A dummy is a simple implementation that does nothing and is only used to satisfy interface requirements.

```csharp
public class DummyMailer : IMailer
{
    public void Send(Message message)
    {
        // No operation
    }
}
```

### Fake

A fake is a working but simplified implementation, often using in-memory logic. It's useful for simulating a real component in a controlled way.

```csharp
public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _customers = new();

    public void Store(Customer customer)
    {
        _customers[customer.Id] = customer;
    }

    public Customer Get(Guid id)
    {
        if (!_customers.TryGetValue(id, out var customer))
            throw new CustomerNotFoundException();

        return customer;
    }

    public Customer FindByEmail(string email)
    {
        return _customers.Values.FirstOrDefault(c => c.Email == email)
               ?? throw new CustomerNotFoundException();
    }
}

```

### Stub

A stub returns predefined responses to method calls.

```csharp
public class AlwaysUniqueEmailStub : IUniqueEmailSpecification
{
    public bool IsUnique(string email) => true;
}
```

Alternatively, using a mocking library like Moq:

```csharp
var stub = new Mock<IUniqueEmailSpecification>();
stub.Setup(x => x.IsUnique(It.IsAny<string>())).Returns(true);

```

### Spy

A spy records information about interactions, which can be asserted later.

```csharp
public class SpyMailer : IMailer
{
    public List<Message> SentMessages { get; } = new();

    public void Send(Message message)
    {
        SentMessages.Add(message);
    }

    public int SentCount => SentMessages.Count;
}

```

### Mock

A mock is used to verify specific interactions, such as method calls and arguments.

```csharp
var mockMailer = new Mock<IMailer>();
var message = new Message("test@example.com", "Test");

mockMailer.Setup(m => m.Send(It.Is<Message>(msg => msg.Equals(message)))).Verifiable();

// System under test uses mockMailer.Object

mockMailer.Verify(m => m.Send(It.IsAny<Message>()), Times.Once);
```

Use stubs for controlling inputs and mocks for verifying outputs.

### Example: Not Recommended (Overuse of Mocks)

```csharp
[Test]
public void Sends_All_Notifications()
{
    var message1 = new Message();
    var message2 = new Message();

    var repo = new Mock<IMessageRepository>();
    repo.Setup(x => x.GetAll()).Returns(new[] { message1, message2 });

    var mailer = new Mock<IMailer>();

    var sut = new NotificationService(mailer.Object, repo.Object);

    sut.Send();

    mailer.Verify(x => x.Send(message1), Times.Once);
    mailer.Verify(x => x.Send(message2), Times.Once);
}
```

### Example: Recommended (Using Fakes and Spies)

```csharp
[Test]
public void Sends_All_Notifications()
{
    var message1 = new Message();
    var message2 = new Message();

    var repository = new InMemoryMessageRepository();
    repository.Save(message1);
    repository.Save(message2);

    var mailer = new SpyMailer();

    var sut = new NotificationService(mailer, repository);
    sut.Send();

    CollectionAssert.AreEquivalent(new[] { message1, message2 }, mailer.SentMessages);
}
```

Advantages:

- Easier to maintain and refactor
- More expressive and readable
- Lower coupling to mocking framework
- More resilient to method signature changes

### Test Naming Best Practices

### Poor Examples

```csharp
[Test]
public void Test() { }

[Test]
public void TestDeactivateSubscription() { }

[Test]
public void ItThrowsWhenPasswordTooShort() { }
```

### Recommended Style

Use descriptive, behavior-focused names. Prefer underscores for readability.

```csharp
[Test]
public void SignIn_WithInvalidCredentials_IsNotPossible() { }

[Test]
public void Creating_WithTooShortPassword_IsInvalid() { }

[Test]
public void Deactivating_AnActiveSubscription_Succeeds() { }

[Test]
public void Deactivating_AnInactiveSubscription_IsInvalid() { }
```

Guidelines:

- Avoid technical terms in test names
- Describe expected behavior clearly
- Use present tense and readable language
- Make it understandable to a non-developer if possible
