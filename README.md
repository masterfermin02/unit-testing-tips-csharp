# Unit Testing Tips in C#

In these times, the benefits of writing unit tests are huge. Most recently started projects contain unit tests. In enterprise applications with a lot of business logic, unit tests are the most important tests, because they are fast and can instantly assure that our implementation is correct. However, I often see poor tests in projects — the benefits of unit testing are only realized when you have **good** unit tests. This guide shares best practices with C# examples to help you write tests that are valuable, maintainable, and resilient to refactoring.

> Adapted from [sarvendev/unit-testing-tips](https://github.com/sarven/unit-testing-tips) — PHP original by Sarven Dev

**Stack used in examples:**
- [xUnit](https://xunit.net/) — test framework
- [Moq](https://github.com/devlooped/moq) — mocking library
- [FluentAssertions](https://fluentassertions.com/) — readable assertions

---

## Table of Contents

1. [Test Doubles](#1-test-doubles)
2. [Test Naming Best Practices](#2-test-naming-best-practices)
3. [AAA Pattern](#3-aaa-pattern)
4. [Object Mother](#4-object-mother)
5. [Builder Pattern](#5-builder-pattern)
6. [Assert Object](#6-assert-object)
7. [Parameterized Tests](#7-parameterized-tests)
8. [Two Schools of Unit Testing](#8-two-schools-of-unit-testing)
9. [Mock vs Stub](#9-mock-vs-stub)
10. [Three Testing Styles](#10-three-testing-styles)
11. [Functional Architecture and Tests](#11-functional-architecture-and-tests)
12. [Observable Behavior vs Implementation Details](#12-observable-behavior-vs-implementation-details)
13. [Unit of Behavior](#13-unit-of-behavior)
14. [Humble Object Pattern](#14-humble-object-pattern)
15. [Trivial Tests](#15-trivial-tests)
16. [Fragile Tests](#16-fragile-tests)
17. [Test Fixtures / Setup](#17-test-fixtures--setup)
18. [Anti-Patterns](#18-anti-patterns)
19. [Test Coverage](#19-test-coverage)

---

## 1. Test Doubles

Test doubles are fake dependencies used in tests. They replace real collaborators so you can test a unit in isolation.

![Test doubles](./assets/test-doubles.jpg ':size=800')

There are five types of test doubles:

| Type | Purpose |
|------|---------|
| Dummy | Satisfies a parameter/interface requirement; never actually used |
| Fake | Working but simplified implementation (e.g., in-memory repository) |
| Stub | Returns predefined responses to method calls |
| Spy | Records interactions that can be asserted later |
| Mock | Pre-configured to verify specific interactions |

---

### Dummy

A dummy satisfies an interface requirement but its methods are never called during the test.

```csharp
public class DummyMailer : IMailer
{
    public void Send(Message message)
    {
        // Intentionally empty — this dependency is required but unused in this test
    }
}
```

---

### Fake

A fake is a working but simplified implementation. It simulates the real component without the heavy infrastructure.

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
            throw new CustomerNotFoundException(id);

        return customer;
    }

    public Customer? FindByEmail(string email)
    {
        return _customers.Values.FirstOrDefault(c => c.Email == email);
    }
}
```

---

### Stub

A stub returns predefined responses to control the system under test's behavior.

**Custom stub class (preferred):**

```csharp
public class AlwaysUniqueEmailStub : IUniqueEmailSpecification
{
    public bool IsUnique(string email) => true;
}

public class NeverUniqueEmailStub : IUniqueEmailSpecification
{
    public bool IsUnique(string email) => false;
}
```

**Using Moq:**

```csharp
var stub = new Mock<IUniqueEmailSpecification>();
stub.Setup(x => x.IsUnique(It.IsAny<string>())).Returns(true);
```

---

### Spy

A spy records interactions so you can assert on them after the act phase.

```csharp
public class SpyMailer : IMailer
{
    public List<Message> SentMessages { get; } = new();

    public void Send(Message message)
    {
        SentMessages.Add(message);
    }

    public int SentCount => SentMessages.Count;

    public bool WasSentTo(string email) =>
        SentMessages.Any(m => m.To == email);
}
```

---

### Mock

A mock is a pre-configured object that verifies specific interactions occurred.

```csharp
var mockMailer = new Mock<IMailer>();
var expectedMessage = new Message("user@example.com", "Welcome!");

// Act: system uses mockMailer.Object internally

mockMailer.Verify(m => m.Send(It.Is<Message>(msg => msg.To == "user@example.com")), Times.Once);
```

---

### Prefer Fakes and Spies Over Mocks

**Not Recommended — overuse of mocks:**

```csharp
[Fact]
public void Sends_AllNotifications()
{
    var message1 = new Message("a@example.com", "Hello");
    var message2 = new Message("b@example.com", "Hello");

    var repo = new Mock<IMessageRepository>();
    repo.Setup(x => x.GetAll()).Returns(new[] { message1, message2 });

    var mailer = new Mock<IMailer>();

    var sut = new NotificationService(mailer.Object, repo.Object);
    sut.Send();

    mailer.Verify(x => x.Send(message1), Times.Once);
    mailer.Verify(x => x.Send(message2), Times.Once);
}
```

**Recommended — using fakes and spies:**

```csharp
[Fact]
public void Sends_AllNotifications()
{
    var message1 = new Message("a@example.com", "Hello");
    var message2 = new Message("b@example.com", "Hello");

    var repository = new InMemoryMessageRepository();
    repository.Save(message1);
    repository.Save(message2);

    var mailer = new SpyMailer();
    var sut = new NotificationService(mailer, repository);

    sut.Send();

    mailer.SentMessages.Should().BeEquivalentTo(new[] { message1, message2 });
}
```

**Why this is better:**
- Easier to maintain and refactor
- More expressive and readable
- Lower coupling to the mocking framework
- More resilient to method signature changes

---

## 2. Test Naming Best Practices

Good test names describe the expected behavior in plain language. They should be understandable even to non-developers.

### Poor Examples

```csharp
[Fact]
public void Test() { }

[Fact]
public void TestDeactivateSubscription() { }

[Fact]
public void ItThrowsWhenPasswordTooShort() { }

[Fact]
public void DeactivateSubscription_ShouldSetStatusToInactive() { }
```

### Recommended Style

Use the pattern: `Subject_Scenario_ExpectedBehavior` or describe the behavior as a readable sentence using underscores.

```csharp
[Fact]
public void SigningIn_WithInvalidCredentials_IsNotPossible() { }

[Fact]
public void Creating_WithTooShortPassword_IsInvalid() { }

[Fact]
public void Deactivating_AnActiveSubscription_Succeeds() { }

[Fact]
public void Deactivating_AnInactiveSubscription_IsInvalid() { }

[Fact]
public void Purchasing_SubscriptionPlan_StartsANewSubscription() { }

[Fact]
public void Upgrading_ToAHigherPlan_IncreasesMonthlyPrice() { }
```

### Guidelines

- **No technical terms** (`Test`, `Assert`, `Should`, `Verify`)
- **Describe behavior**, not implementation
- **Use present tense** or readable active language
- **Underscores** improve readability over camelCase for test names
- **Non-developers** should be able to read the name and understand the scenario
- Name the variable holding the system under test **`sut`**

```csharp
[Fact]
public void Deactivating_AnActiveSubscription_Succeeds()
{
    // The SUT (System Under Test) is clearly labeled
    var sut = new Subscription(SubscriptionStatus.Active);

    sut.Deactivate();

    sut.Status.Should().Be(SubscriptionStatus.Inactive);
}
```

---

## 3. AAA Pattern

Structure every test into three sections: **Arrange**, **Act**, **Assert**.

```csharp
[Fact]
public void Deactivating_AnActiveSubscription_Succeeds()
{
    // Arrange
    var subscription = new Subscription(SubscriptionStatus.Active);

    // Act
    subscription.Deactivate();

    // Assert
    subscription.Status.Should().Be(SubscriptionStatus.Inactive);
}
```

### Guidelines

- Keep each section focused and minimal
- The **Arrange** phase sets up the SUT and its dependencies
- The **Act** phase invokes the behavior being tested — ideally a single method call
- The **Assert** phase verifies the outcome (return value, state change, or interaction)
- Avoid multiple act-assert cycles in one test — split them into separate tests

**Avoid — multiple acts:**

```csharp
[Fact]
public void Managing_SubscriptionLifecycle()
{
    var sub = new Subscription(SubscriptionStatus.Active);

    sub.Deactivate();
    sub.Status.Should().Be(SubscriptionStatus.Inactive); // assert 1

    sub.Reactivate();
    sub.Status.Should().Be(SubscriptionStatus.Active); // assert 2 — this is two tests!
}
```

**Prefer — single behavior per test:**

```csharp
[Fact]
public void Deactivating_AnActiveSubscription_SetsStatusToInactive()
{
    var sut = new Subscription(SubscriptionStatus.Active);
    sut.Deactivate();
    sut.Status.Should().Be(SubscriptionStatus.Inactive);
}

[Fact]
public void Reactivating_AnInactiveSubscription_SetsStatusToActive()
{
    var sut = new Subscription(SubscriptionStatus.Inactive);
    sut.Reactivate();
    sut.Status.Should().Be(SubscriptionStatus.Active);
}
```

---

## 4. Object Mother

An Object Mother is a factory class that creates pre-configured test objects. It eliminates duplication in test setup and makes tests more readable.

```csharp
public static class SubscriptionMother
{
    public static Subscription New() =>
        new(SubscriptionStatus.New);

    public static Subscription Active() =>
        new(SubscriptionStatus.Active);

    public static Subscription Inactive() =>
        new(SubscriptionStatus.Inactive);

    public static Subscription ActiveWithPlan(SubscriptionPlan plan) =>
        new(SubscriptionStatus.Active, plan);
}
```

**Usage in tests:**

```csharp
[Fact]
public void Deactivating_AnActiveSubscription_Succeeds()
{
    var sut = SubscriptionMother.Active();

    sut.Deactivate();

    sut.Status.Should().Be(SubscriptionStatus.Inactive);
}

[Fact]
public void Deactivating_AnInactiveSubscription_IsInvalid()
{
    var sut = SubscriptionMother.Inactive();

    var act = () => sut.Deactivate();

    act.Should().Throw<InvalidOperationException>();
}
```

**Benefits:**
- One place to update if the constructor changes
- Descriptive names communicate intent (e.g., `Active()` vs `new Subscription(...)`)
- Reusable across many test classes

---

## 5. Builder Pattern

The Builder pattern is ideal for creating complex objects with many properties, using a fluent interface.

```csharp
public class OrderBuilder
{
    private DateTime _createdAt = DateTime.UtcNow;
    private readonly List<OrderItem> _items = new();
    private CustomerId _customerId = new CustomerId(Guid.NewGuid());

    public OrderBuilder CreatedAt(DateTime date)
    {
        _createdAt = date;
        return this;
    }

    public OrderBuilder ForCustomer(CustomerId customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithItem(string name, decimal price, int quantity = 1)
    {
        _items.Add(new OrderItem(name, new Money(price), quantity));
        return this;
    }

    public Order Build() => new(_customerId, _createdAt, _items);
}
```

**Usage in tests:**

```csharp
[Fact]
public void CalculatingTotal_WithMultipleItems_SumsAllPrices()
{
    var order = new OrderBuilder()
        .CreatedAt(new DateTime(2024, 1, 15))
        .WithItem("Widget", 19.99m)
        .WithItem("Gadget", 49.99m, quantity: 2)
        .Build();

    order.Total.Amount.Should().Be(119.97m);
}
```

**Object Mother vs Builder:**
- Use **Object Mother** for a small set of well-known named states (`Active()`, `Inactive()`)
- Use **Builder** when tests need objects with many varying properties

---

## 6. Assert Object

An Assert Object (Asserter) wraps assertions in a fluent, domain-language interface. It makes assertions readable and reusable across tests.

```csharp
public class OrderAsserter
{
    private readonly Order _order;

    private OrderAsserter(Order order)
    {
        _order = order;
    }

    public static OrderAsserter AssertThat(Order order) => new(order);

    public OrderAsserter WasCreatedAt(DateTime expectedDate)
    {
        _order.CreatedAt.Should().Be(expectedDate);
        return this;
    }

    public OrderAsserter HasTotal(decimal expectedTotal)
    {
        _order.Total.Amount.Should().Be(expectedTotal);
        return this;
    }

    public OrderAsserter HasItemCount(int expectedCount)
    {
        _order.Items.Should().HaveCount(expectedCount);
        return this;
    }

    public OrderAsserter ContainsItem(string name)
    {
        _order.Items.Should().Contain(i => i.Name == name);
        return this;
    }
}
```

**Usage in tests:**

```csharp
[Fact]
public void PlacingOrder_WithItems_CreatesOrderCorrectly()
{
    var createdAt = new DateTime(2024, 6, 1);

    var order = new OrderBuilder()
        .CreatedAt(createdAt)
        .WithItem("Widget", 10.00m)
        .WithItem("Gadget", 20.00m)
        .Build();

    OrderAsserter.AssertThat(order)
        .WasCreatedAt(createdAt)
        .HasTotal(30.00m)
        .HasItemCount(2)
        .ContainsItem("Widget");
}
```

---

## 7. Parameterized Tests

Use parameterized tests to avoid code duplication when testing multiple scenarios with the same structure. In xUnit, use `[Theory]` and `[InlineData]` or `[MemberData]`.

### Basic Parameterized Tests

```csharp
[Theory]
[InlineData("")]
[InlineData("ab")]
[InlineData("1234567")]
public void Creating_WithTooShortPassword_IsInvalid(string password)
{
    var act = () => new User("user@example.com", password);

    act.Should().Throw<ArgumentException>()
        .WithMessage("*password*");
}
```

### Separate Positive and Negative Cases

**Not Recommended — mixing success and failure in one test:**

```csharp
[Theory]
[InlineData("validpassword123", true)]
[InlineData("short", false)]
public void ValidatingPassword(string password, bool isValid) { }
```

**Recommended — separate theories for positive and negative:**

```csharp
[Theory]
[InlineData("validpassword123")]
[InlineData("another_valid_pass!")]
[InlineData("LongEnoughPassword1")]
public void Creating_WithValidPassword_Succeeds(string password)
{
    var act = () => new User("user@example.com", password);

    act.Should().NotThrow();
}

[Theory]
[InlineData("")]
[InlineData("short")]
[InlineData("1234567")]
public void Creating_WithTooShortPassword_IsInvalid(string password)
{
    var act = () => new User("user@example.com", password);

    act.Should().Throw<ArgumentException>();
}
```

### Using MemberData for Complex Objects

```csharp
public class DiscountTests
{
    public static IEnumerable<object[]> InvalidDiscountData => new[]
    {
        new object[] { -1m, "Discount cannot be negative" },
        new object[] { 101m, "Discount cannot exceed 100%" },
        new object[] { 0m, "Discount must be greater than zero" },
    };

    [Theory]
    [MemberData(nameof(InvalidDiscountData))]
    public void ApplyingDiscount_WithInvalidValue_ThrowsException(decimal discount, string reason)
    {
        var order = new OrderBuilder().WithItem("Widget", 100m).Build();

        var act = () => order.ApplyDiscount(discount);

        act.Should().Throw<ArgumentException>(because: reason);
    }
}
```

---

## 8. Two Schools of Unit Testing

There are two main approaches to unit testing, each with different definitions of a "unit."

### Classical School (Detroit / Chicago)

A **unit** is a single unit of *behavior* — it can span multiple related classes. Tests verify the observable outcome of a business operation.

```csharp
[Fact]
public void Purchasing_ASubscriptionPlan_StartsANewSubscription()
{
    // Arrange — multiple real classes collaborate
    var customer = CustomerMother.Active();
    var plan = SubscriptionPlanMother.Monthly();
    var repository = new InMemorySubscriptionRepository();
    var sut = new SubscriptionService(repository);

    // Act
    sut.Purchase(customer, plan);

    // Assert — verify observable result
    var subscription = repository.FindByCustomer(customer.Id);
    subscription.Should().NotBeNull();
    subscription!.Status.Should().Be(SubscriptionStatus.Active);
}
```

### Mockist School (London)

A **unit** is a single *class*. All collaborators are replaced with mocks, and the test verifies the interactions.

```csharp
[Fact]
public void Purchasing_ASubscriptionPlan_StoresSubscription()
{
    // All collaborators are mocked
    var customer = CustomerMother.Active();
    var plan = SubscriptionPlanMother.Monthly();
    var mockRepository = new Mock<ISubscriptionRepository>();

    var sut = new SubscriptionService(mockRepository.Object);
    sut.Purchase(customer, plan);

    mockRepository.Verify(r => r.Save(It.IsAny<Subscription>()), Times.Once);
}
```

### Comparison

| | Classical | Mockist |
|---|---|---|
| Unit definition | Unit of behavior | Single class |
| Collaborators | Real objects or fakes | All mocked |
| Refactoring resistance | High | Low (fragile) |
| Test isolation | Moderate | Complete |
| Fragility risk | Lower | Higher |

**Recommendation:** Prefer the **Classical school**. Tests that don't depend on implementation details are more resistant to refactoring and provide higher value.

---

## 9. Mock vs Stub

Understanding when to use a mock vs a stub is crucial for writing non-fragile tests.

- **Stub** → controls *input* into the SUT (what the SUT receives from collaborators)
- **Mock** → verifies *output* from the SUT (what the SUT sends to collaborators)

```
[Collaborator] --stub--> [SUT] --mock--> [Collaborator]
   (incoming)                              (outgoing)
```

### Use Stubs for Incoming Interactions

```csharp
[Fact]
public void GettingCustomer_WhenExists_ReturnsCorrectCustomer()
{
    // Stub controls what the SUT receives
    var existingCustomer = CustomerMother.Active();
    var stub = new InMemoryCustomerRepository();
    stub.Store(existingCustomer);

    var sut = new CustomerService(stub);

    var result = sut.Get(existingCustomer.Id);

    result.Should().BeEquivalentTo(existingCustomer);
}
```

### Use Mocks for Outgoing Interactions

```csharp
[Fact]
public void Registering_NewUser_SendsWelcomeEmail()
{
    // Stub for incoming (data source)
    var emailSpec = new AlwaysUniqueEmailStub();
    // Spy/mock for outgoing (side effect verification)
    var mailer = new SpyMailer();

    var sut = new UserRegistrationService(emailSpec, mailer);
    sut.Register("new@example.com", "securepassword123");

    mailer.WasSentTo("new@example.com").Should().BeTrue();
}
```

### Never Assert on Stub Interactions

**Wrong — asserting on a stub:**

```csharp
[Fact]
public void GettingSubscription_CallsRepositoryOnce()
{
    var mockRepo = new Mock<ISubscriptionRepository>();
    mockRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(SubscriptionMother.Active());

    var sut = new SubscriptionService(mockRepo.Object);
    sut.Get(Guid.NewGuid());

    // This is wrong! The repository is an input (stub), not an output (mock).
    // Asserting on it couples tests to implementation details.
    mockRepo.Verify(r => r.Get(It.IsAny<Guid>()), Times.Once);
}
```

---

## 10. Three Testing Styles

Tests can verify three different things. They differ significantly in quality and maintainability.

### 1. Output Testing (Best)

Tests the **return value** of a method. No side effects needed.

```csharp
[Fact]
public void CalculatingDiscount_ForPremiumCustomer_Returns20Percent()
{
    var sut = new DiscountCalculator();

    var discount = sut.Calculate(CustomerTier.Premium, orderAmount: 100m);

    discount.Should().Be(20m);
}
```

**Characteristics:**
- Highest refactoring resistance
- No need to know internal structure
- Pure functions are always output-testable
- Zero maintenance cost as long as behavior stays the same

### 2. State Testing (Middle)

Tests the **state of an object** after an operation.

```csharp
[Fact]
public void AddingItemToCart_IncreasesItemCount()
{
    var sut = new ShoppingCart();
    var item = new CartItem("Widget", 9.99m);

    sut.Add(item);

    sut.Items.Should().HaveCount(1);
    sut.Items.Should().ContainSingle(i => i.Name == "Widget");
}
```

**Characteristics:**
- Good when the result is a state change, not a return value
- Slightly more coupled to object structure than output testing
- Avoid exposing state solely for tests

### 3. Communication Testing (Worst)

Tests the **interactions between objects** (method calls, arguments passed).

```csharp
[Fact]
public void ProcessingOrder_NotifiesCustomer()
{
    var mockMailer = new Mock<IMailer>();
    var sut = new OrderProcessor(mockMailer.Object);

    sut.Process(OrderMother.Completed());

    // Testing that a specific method was called with specific args
    mockMailer.Verify(m => m.Send(It.Is<Message>(msg =>
        msg.Subject == "Order Processed")), Times.Once);
}
```

**Characteristics:**
- Lowest refactoring resistance — any internal change breaks the test
- High coupling to implementation
- Use sparingly and only for true "outgoing command" interactions
- Never use for queries/reads

**Ranking Summary:**

| Style | Refactoring Resistance | Maintenance Cost | When to Use |
|-------|----------------------|-----------------|-------------|
| Output | Highest | Lowest | Always prefer |
| State | Medium | Medium | When no return value |
| Communication | Lowest | Highest | Only for side effects |

---

## 11. Functional Architecture and Tests

Separate code with side effects from pure business logic. This makes the core logic testable via fast unit tests, while infrastructure is covered by integration tests.

### Anti-Pattern — Logic Mixed with Side Effects

```csharp
// Hard to unit test — reads from disk AND calculates
public class ReportService
{
    public Report Generate(string filePath)
    {
        var lines = File.ReadAllLines(filePath); // side effect
        var data = lines.Select(ParseLine).ToList(); // pure logic
        return new Report(data.Sum(d => d.Amount)); // pure logic
    }
}
```

### Recommended — Separate Layers

```csharp
// Pure logic — easily unit tested
public class ReportCalculator
{
    public Report Calculate(IEnumerable<ReportLine> lines)
    {
        return new Report(lines.Sum(l => l.Amount));
    }
}

// Side effects — tested via integration tests
public class ReportFileLoader
{
    public IEnumerable<ReportLine> Load(string filePath)
    {
        return File.ReadAllLines(filePath).Select(ParseLine);
    }

    private static ReportLine ParseLine(string line) { /* ... */ }
}

// Orchestration — thin, tested via integration tests
public class ReportService
{
    private readonly ReportFileLoader _loader;
    private readonly ReportCalculator _calculator;

    public ReportService(ReportFileLoader loader, ReportCalculator calculator)
    {
        _loader = loader;
        _calculator = calculator;
    }

    public Report Generate(string filePath)
    {
        var lines = _loader.Load(filePath);
        return _calculator.Calculate(lines);
    }
}
```

**Unit tests for pure logic:**

```csharp
[Fact]
public void Calculating_ReportWithMultipleLines_SumsTotals()
{
    var lines = new[]
    {
        new ReportLine("Sales", 1000m),
        new ReportLine("Returns", -200m),
        new ReportLine("Adjustments", 50m),
    };

    var sut = new ReportCalculator();
    var report = sut.Calculate(lines);

    report.Total.Should().Be(850m);
}
```

**Architecture:**
```
[FileLoader] ──────► [ApplicationService] ◄────── [Calculator]
(side effects)       (thin orchestration)          (pure logic)
 integration tests                                  unit tests
```

---

## 12. Observable Behavior vs Implementation Details

Tests should only verify what the class **exposes as its contract** — not how it works internally.

### Anti-Pattern — Testing Implementation Details

```csharp
// Production code
public class PasswordHasher
{
    private string _salt; // internal detail

    public string Hash(string password)
    {
        _salt = GenerateSalt();
        return Sha256(password + _salt);
    }
}

// Bad test — exposes internals via reflection
[Fact]
public void Hashing_SetsInternalSalt()
{
    var sut = new PasswordHasher();
    sut.Hash("password123");

    var salt = typeof(PasswordHasher)
        .GetField("_salt", BindingFlags.NonPublic | BindingFlags.Instance)!
        .GetValue(sut);

    salt.Should().NotBeNull(); // Testing an internal detail!
}
```

### Recommended — Test Observable Behavior

```csharp
[Fact]
public void Hashing_SamePassword_Twice_ProducesDifferentHashes()
{
    var sut = new PasswordHasher();

    var hash1 = sut.Hash("password123");
    var hash2 = sut.Hash("password123");

    // Observable: hashes differ (implies salting works)
    hash1.Should().NotBe(hash2);
}

[Fact]
public void Verifying_CorrectPassword_ReturnsTrue()
{
    var sut = new PasswordHasher();
    var hash = sut.Hash("password123");

    var result = sut.Verify("password123", hash);

    result.Should().BeTrue();
}
```

### Do Not Add Getters Solely for Tests

**Bad — exposing state only for testing:**

```csharp
public class Subscription
{
    private DateTime _renewalDate;

    // Don't add this just for testing!
    public DateTime RenewalDate => _renewalDate;

    public void Renew() { _renewalDate = DateTime.UtcNow.AddMonths(1); }
}
```

**Good — verify through domain behavior:**

```csharp
[Fact]
public void Renewing_ASubscription_AllowsAccessForAnotherMonth()
{
    var clock = new FixedClock(new DateTime(2024, 1, 1));
    var sut = new Subscription(SubscriptionStatus.Active, clock);

    sut.Renew();

    // Verify behavior, not internal state:
    sut.IsActiveAt(new DateTime(2024, 1, 31)).Should().BeTrue();
    sut.IsActiveAt(new DateTime(2024, 2, 2)).Should().BeFalse();
}
```

---

## 13. Unit of Behavior

A test should verify a **unit of behavior**, not a unit of code (one class = one test file = one test per method).

### Anti-Pattern — 1:1 Mapping to Methods

```csharp
// Test mirrors the implementation — brittle, low value
public class SubscriptionTests
{
    [Fact] public void SetStatus_Works() { }
    [Fact] public void SetRenewalDate_Works() { }
    [Fact] public void SetPlan_Works() { }
}
```

### Recommended — Test Business Scenarios

```csharp
public class SubscriptionTests
{
    [Fact]
    public void Purchasing_MonthlySub_StartsActiveSubscription() { }

    [Fact]
    public void Purchasing_MonthlySub_SetsRenewalDateOneMonthAhead() { }

    [Fact]
    public void Renewing_BeforeExpiry_ExtendsRenewalDate() { }

    [Fact]
    public void Deactivating_AnActiveSubscription_PreventsAccess() { }

    [Fact]
    public void Upgrading_ToAnnualPlan_UpdatesPriceAndRenewalDate() { }
}
```

This approach:
- Survives internal refactoring (renaming private methods, extracting helpers)
- Documents the business rules clearly
- Reduces test count without losing coverage of important behavior

---

## 14. Humble Object Pattern

When a class mixes complex logic with difficult-to-test infrastructure (I/O, UI, external APIs), extract the logic into a separate, easily testable class.

### Problem — Logic Buried in Infrastructure

```csharp
public class OrderController : ControllerBase
{
    [HttpPost]
    public IActionResult PlaceOrder(PlaceOrderRequest request)
    {
        // Business logic mixed into controller — hard to unit test
        if (request.Items.Count == 0)
            return BadRequest("Order must have items");

        var total = request.Items.Sum(i => i.Price * i.Quantity);
        if (total > 10000m)
            return BadRequest("Order total exceeds limit");

        // ... more business logic
    }
}
```

### Recommended — Extract Pure Logic

```csharp
// Humble object — thin, hard to test (controller/UI/infrastructure)
public class OrderController : ControllerBase
{
    private readonly OrderService _service;

    [HttpPost]
    public IActionResult PlaceOrder(PlaceOrderRequest request)
    {
        var result = _service.Place(request.ToCommand());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

// Domain service — pure logic, easily unit tested
public class OrderService
{
    public Result<OrderId> Place(PlaceOrderCommand command)
    {
        if (!command.Items.Any())
            return Result.Failure<OrderId>("Order must have items");

        var total = command.Items.Sum(i => i.Price * i.Quantity);
        if (total > 10000m)
            return Result.Failure<OrderId>("Order total exceeds limit");

        var order = new Order(command.CustomerId, command.Items);
        return Result.Success(order.Id);
    }
}
```

**Unit test the pure service:**

```csharp
[Fact]
public void PlacingOrder_WithNoItems_Fails()
{
    var sut = new OrderService();

    var result = sut.Place(new PlaceOrderCommand(CustomerId.New(), Items: []));

    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("items");
}
```

---

## 15. Trivial Tests

Not all code needs unit tests. Avoid testing code that has no meaningful logic.

### Skip Tests For

```csharp
// Simple property — no logic
public class Customer
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// Constant-returning method
public class AppConfig
{
    public string GetEnvironment() => "production";
}

// Pure data containers / DTOs
public record OrderDto(Guid Id, decimal Total, DateTime CreatedAt);
```

### Test Code That Has Real Logic

```csharp
// Has logic — worth testing
public class DiscountCalculator
{
    public decimal Calculate(CustomerTier tier, decimal amount)
    {
        return tier switch
        {
            CustomerTier.Premium => amount * 0.20m,
            CustomerTier.Standard => amount >= 100m ? amount * 0.05m : 0m,
            _ => 0m
        };
    }
}
```

> **Rule of thumb:** If you can describe the test as "it calls the method and returns the value I set", it's testing nothing.

---

## 16. Fragile Tests

Fragile tests break when the implementation changes but the behavior hasn't. They create noise, reduce trust in the test suite, and slow down refactoring.

### Common Causes of Fragility

**1. Testing internal implementation through mocks:**

```csharp
// Fragile — tightly coupled to which private method is called
[Fact]
public void ProcessingOrder_CallsValidatorThenPersister()
{
    var mockValidator = new Mock<IOrderValidator>();
    var mockPersister = new Mock<IOrderPersister>();

    var sut = new OrderProcessor(mockValidator.Object, mockPersister.Object);
    sut.Process(OrderMother.Valid());

    // If we inline the validator, this test breaks — even though behavior is unchanged
    mockValidator.Verify(v => v.Validate(It.IsAny<Order>()), Times.Once);
    mockPersister.Verify(p => p.Save(It.IsAny<Order>()), Times.Once);
}
```

**Better — test the observable outcome:**

```csharp
[Fact]
public void ProcessingOrder_PersistsSuccessfully()
{
    var repository = new InMemoryOrderRepository();
    var sut = new OrderProcessor(repository);

    sut.Process(OrderMother.Valid());

    repository.Count.Should().Be(1);
}
```

**2. Mocking database repositories (use integration tests instead):**

```csharp
// Bad — faking a DB repository in a unit test gives false confidence
var mockRepo = new Mock<IOrderRepository>();
mockRepo.Setup(r => r.GetPendingOrders()).Returns(testOrders);
```

**3. Hard-coding time:**

```csharp
// Fragile — test will fail tomorrow if it checks for "today"
var order = new Order(customerId, DateTime.Now);
order.CreatedAt.Date.Should().Be(DateTime.Today); // passes today, fragile tomorrow
```

---

## 17. Test Fixtures / Setup

Avoid putting all test state into `SetUp`/constructor when it doesn't apply to every test.

### Avoid — Shared Setup That Doesn't Fit All Tests

```csharp
public class SubscriptionTests
{
    private readonly Subscription _subscription;
    private readonly SpyMailer _mailer;

    public SubscriptionTests()
    {
        // Constructs state that only some tests need
        _subscription = SubscriptionMother.Active();
        _mailer = new SpyMailer();
    }

    [Fact]
    public void Deactivating_ActiveSubscription_Succeeds()
    {
        // Uses _subscription — OK
        _subscription.Deactivate();
        _subscription.Status.Should().Be(SubscriptionStatus.Inactive);
    }

    [Fact]
    public void Creating_NewSubscription_HasNewStatus()
    {
        // _subscription is Active here — misleading for this test!
        var sut = new Subscription(SubscriptionStatus.New);
        sut.Status.Should().Be(SubscriptionStatus.New);
    }
}
```

### Recommended — Private Helper Methods per Test

```csharp
public class SubscriptionTests
{
    [Fact]
    public void Deactivating_ActiveSubscription_Succeeds()
    {
        var sut = CreateActiveSubscription();

        sut.Deactivate();

        sut.Status.Should().Be(SubscriptionStatus.Inactive);
    }

    [Fact]
    public void Creating_NewSubscription_HasNewStatus()
    {
        var sut = new Subscription(SubscriptionStatus.New);

        sut.Status.Should().Be(SubscriptionStatus.New);
    }

    private static Subscription CreateActiveSubscription() =>
        SubscriptionMother.Active();
}
```

**Recommended uses for `constructor` / `SetUp`:**
- Stateless objects used by all tests (e.g., `_sut = new PriceCalculator()`)
- Shared infrastructure that's always needed and stateless

---

## 18. Anti-Patterns

### 18.1 Exposing Private State

**Bad — adding getters purely for tests:**

```csharp
// Production code polluted for testability
public class Invoice
{
    private decimal _taxAmount;

    public decimal TaxAmount => _taxAmount; // added only for testing!

    public decimal Calculate(decimal subtotal, decimal taxRate)
    {
        _taxAmount = subtotal * taxRate;
        return subtotal + _taxAmount;
    }
}

[Fact]
public void Calculating_Invoice_SetsTaxAmount()
{
    var sut = new Invoice();
    sut.Calculate(100m, 0.20m);
    sut.TaxAmount.Should().Be(20m); // Tests internal state — fragile!
}
```

**Good — test observable outcome:**

```csharp
[Fact]
public void Calculating_Invoice_ReturnsTotalIncludingTax()
{
    var sut = new Invoice();

    var total = sut.Calculate(subtotal: 100m, taxRate: 0.20m);

    total.Should().Be(120m); // Tests what the caller cares about
}
```

---

### 18.2 Leaking Domain Details

**Bad — duplicating production logic in tests:**

```csharp
[Fact]
public void Calculating_Discount_IsCorrect()
{
    const decimal price = 100m;
    const decimal discountRate = 0.15m;

    var sut = new DiscountService();
    var result = sut.Apply(price, discountRate);

    // Leaking the formula — if the formula changes, both prod and test change
    result.Should().Be(price - (price * discountRate));
}
```

**Good — use hardcoded expected values:**

```csharp
[Fact]
public void Applying_15PercentDiscount_To100Dollars_Returns85Dollars()
{
    var sut = new DiscountService();

    var result = sut.Apply(price: 100m, discountRate: 0.15m);

    result.Should().Be(85m); // Explicit, hardcoded — catches formula bugs
}
```

---

### 18.3 Mocking Concrete Classes

**Bad — mocking a concrete class:**

```csharp
// Moq can mock concrete classes but this indicates a design smell
var mockEmailSender = new Mock<SmtpEmailSender>();
mockEmailSender.Setup(s => s.Send(It.IsAny<Email>()));
```

**Good — introduce an interface:**

```csharp
public interface IEmailSender
{
    void Send(Email email);
}

public class SmtpEmailSender : IEmailSender
{
    public void Send(Email email) { /* SMTP implementation */ }
}

// Test double against the interface
public class FakeEmailSender : IEmailSender
{
    public List<Email> Sent { get; } = new();
    public void Send(Email email) => Sent.Add(email);
}
```

> If you find yourself needing to mock a concrete class, consider whether the class violates the Single Responsibility Principle.

---

### 18.4 Testing Private Methods

**Bad — testing via reflection:**

```csharp
[Fact]
public void ValidatingEmail_InternalMethod_IsCorrect()
{
    var sut = new UserService();

    // Using reflection to call private method — never do this!
    var method = typeof(UserService).GetMethod("ValidateEmail",
        BindingFlags.NonPublic | BindingFlags.Instance);

    var result = (bool)method!.Invoke(sut, new object[] { "bad@" })!;

    result.Should().BeFalse();
}
```

**Good — test through public API:**

```csharp
[Fact]
public void Registering_WithInvalidEmail_Fails()
{
    var sut = new UserService(new AlwaysUniqueEmailStub(), new SpyMailer());

    var act = () => sut.Register("bad@", "validpassword123");

    act.Should().Throw<ArgumentException>()
        .WithMessage("*email*");
}
```

> If a private method feels important enough to test directly, it's a signal that it should be extracted into its own class with a public API.

---

### 18.5 Time as a Volatile Dependency

Never call `DateTime.Now` or `DateTime.UtcNow` directly in production code you want to unit test.

**Bad — hard-coded dependency on system clock:**

```csharp
public class SubscriptionService
{
    public void Renew(Subscription subscription)
    {
        subscription.RenewUntil(DateTime.UtcNow.AddMonths(1)); // untestable!
    }
}

[Fact]
public void Renewing_SetsCorrectExpiryDate() // Flaky — depends on when test runs!
{
    var sut = new SubscriptionService();
    var subscription = SubscriptionMother.Active();

    sut.Renew(subscription);

    subscription.ExpiresAt.Date.Should().Be(DateTime.UtcNow.AddMonths(1).Date);
}
```

**Good — inject the clock:**

```csharp
public interface IClock
{
    DateTime UtcNow { get; }
}

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public class FixedClock : IClock
{
    private readonly DateTime _now;
    public FixedClock(DateTime now) => _now = now;
    public DateTime UtcNow => _now;
}

public class SubscriptionService
{
    private readonly IClock _clock;

    public SubscriptionService(IClock clock)
    {
        _clock = clock;
    }

    public void Renew(Subscription subscription)
    {
        subscription.RenewUntil(_clock.UtcNow.AddMonths(1));
    }
}

[Fact]
public void Renewing_SetsExpiryOneMonthFromNow()
{
    var frozenTime = new DateTime(2024, 1, 15);
    var clock = new FixedClock(frozenTime);
    var subscription = SubscriptionMother.Active();
    var sut = new SubscriptionService(clock);

    sut.Renew(subscription);

    subscription.ExpiresAt.Should().Be(new DateTime(2024, 2, 15));
}
```

---

## 19. Test Coverage

100% code coverage is **not** a goal — and chasing it can actually make tests worse.

### Why 100% Coverage Is Misleading

```csharp
// 100% covered — but tests nothing meaningful
[Fact]
public void GetName_ReturnsName()
{
    var customer = new Customer { Name = "Alice" };
    customer.Name.Should().Be("Alice"); // trivial getter
}

// This test adds coverage but not confidence
[Fact]
public void Constructor_SetsId()
{
    var id = Guid.NewGuid();
    var customer = new Customer(id, "Alice");
    customer.Id.Should().Be(id); // trivially true
}
```

### Better Goal — Mutation Testing

Mutation testing tools (like [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)) modify your code and check whether your tests catch the change.

```bash
# Install Stryker.NET
dotnet tool install -g dotnet-stryker

# Run mutation tests
dotnet stryker
```

A mutant is "killed" when your test fails after the mutation. High mutation score means your tests actually verify logic.

### Guidelines

- Focus on **business logic** — domain services, calculations, state transitions
- Use **integration tests** for infrastructure (repositories, external APIs)
- Ignore trivial code: simple getters/setters, DTOs, auto-generated code
- A test suite with **70% meaningful coverage** beats one with **100% trivial coverage**
- Treat a failing test as **signal**, not noise — fragile tests that break on refactoring indicate over-specified tests

---

## Recommended Resources

- **[Unit Testing Principles, Practices, and Patterns](https://www.manning.com/books/unit-testing)** by Vladimir Khorikov — the definitive book on this topic
- **[Test Driven Development: By Example](https://www.oreilly.com/library/view/test-driven-development/0321146530/)** by Kent Beck
- **[xUnit documentation](https://xunit.net/docs/getting-started/netcore/cmdline)** — official xUnit docs
- **[FluentAssertions](https://fluentassertions.com/introduction)** — readable assertion library
- **[Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)** — mutation testing for .NET

---

## Running the Examples

```bash
# Navigate to the solution
cd src

# Restore packages
dotnet restore

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run mutation tests
dotnet stryker
```
