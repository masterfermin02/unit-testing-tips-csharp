using FluentAssertions;
using UnitTestingTips.Domain.Auth;
using UnitTestingTips.Domain.Orders;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates unit testing anti-patterns and how to fix them.
///
/// Anti-patterns covered:
///   18.1 Exposing Private State
///   18.2 Leaking Domain Details
///   18.3 Mocking Concrete Classes
///   18.4 Testing Private Methods
///   18.5 Time as a Volatile Dependency
/// </summary>
public class AntiPatternsTests
{
    // ─────────────────────────────────────────────
    // 18.1 EXPOSING PRIVATE STATE
    // ─────────────────────────────────────────────

    // ❌ Anti-pattern: would require a getter added only for testing
    //
    // [Fact]
    // public void BAD_Renewing_SetsRenewalDateField()
    // {
    //     var sut = SubscriptionMother.Active();
    //     sut.RenewUntil(new DateTime(2024, 3, 1));
    //     sut.RenewalDate.Should().Be(new DateTime(2024, 3, 1)); // needs a public getter for internal state
    // }

    // ✅ Correct: test through observable behavior
    [Fact]
    public void GOOD_Renewing_AllowsAccessUntilNewDate()
    {
        var sut = SubscriptionMother.Active();

        sut.RenewUntil(new DateTime(2024, 3, 1));

        // Observable behavior — no internal getter needed
        sut.IsActiveAt(new DateTime(2024, 2, 28)).Should().BeTrue();
        sut.IsActiveAt(new DateTime(2024, 3, 2)).Should().BeFalse();
    }

    // ─────────────────────────────────────────────
    // 18.2 LEAKING DOMAIN DETAILS
    // ─────────────────────────────────────────────

    // ❌ Anti-pattern: duplicates production formula in test
    [Fact]
    public void BAD_CalculatingDiscount_DuplicatesFormula()
    {
        const decimal price = 100m;
        const decimal discountRate = 0.20m; // Premium rate — leaking business rule into test

        var sut = new DiscountCalculator();
        var discount = sut.Calculate(UnitTestingTips.Domain.Customers.CustomerTier.Premium, price);

        // If formula changes, both production code AND this test need updating
        discount.Should().Be(price * discountRate);
    }

    // ✅ Correct: hardcoded expected value catches formula bugs
    [Fact]
    public void GOOD_CalculatingDiscount_ForPremiumCustomer_Returns20Dollars()
    {
        var sut = new DiscountCalculator();

        var discount = sut.Calculate(UnitTestingTips.Domain.Customers.CustomerTier.Premium, 100m);

        // Hardcoded: if the formula changes, the test catches it
        discount.Should().Be(20m);
    }

    // ─────────────────────────────────────────────
    // 18.3 MOCKING CONCRETE CLASSES
    // ─────────────────────────────────────────────

    // ❌ Anti-pattern: mocking a concrete class (requires virtual methods)
    // var mockCalc = new Mock<DiscountCalculator>();
    // This indicates a design smell — extract an interface instead.

    // ✅ Correct: mock against an interface
    public interface IDiscountCalculator
    {
        decimal Calculate(UnitTestingTips.Domain.Customers.CustomerTier tier, decimal amount);
    }

    // ─────────────────────────────────────────────
    // 18.4 TESTING PRIVATE METHODS
    // ─────────────────────────────────────────────

    // ❌ Anti-pattern: using reflection to test private logic
    //
    // [Fact]
    // public void BAD_PrivateValidation_ViaReflection()
    // {
    //     var sut = new UserRegistrationService(...);
    //     var method = typeof(UserRegistrationService)
    //         .GetMethod("ValidateEmail", BindingFlags.NonPublic | BindingFlags.Instance);
    //     var result = (bool)method.Invoke(sut, new object[] { "bad@" });
    //     result.Should().BeFalse();
    // }

    // ✅ Correct: test through the public API
    [Fact]
    public void GOOD_Registering_WithInvalidEmail_ThrowsThroughPublicAPI()
    {
        var emailSpec = new AlwaysUniqueEmailStub();
        var mailer = new DummyMailer();
        var sut = new UserRegistrationService(emailSpec, mailer);

        var act = () => sut.Register("not-an-email", "validpassword123");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    // ─────────────────────────────────────────────
    // 18.5 TIME AS A VOLATILE DEPENDENCY
    // ─────────────────────────────────────────────

    // ❌ Anti-pattern: DateTime.UtcNow called directly in production code (see domain)
    //
    // public void Renew(Subscription sub)
    // {
    //     sub.RenewUntil(DateTime.UtcNow.AddMonths(1)); // not testable!
    // }

    // ✅ Correct: inject IClock, use FixedClock in tests
    [Fact]
    public void GOOD_Renewing_WithInjectedClock_SetsCorrectExpiry()
    {
        // Arrange — frozen time makes the test deterministic
        var frozenTime = new DateTime(2024, 6, 15);
        var clock = new FixedClock(frozenTime);
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());
        sut.Activate(clock);

        var subscriptionService = new SubscriptionService(
            new InMemorySubscriptionRepository(), clock);

        // Act
        subscriptionService.Renew(sut);

        // Assert — deterministic because clock is fixed
        sut.ExpiresAt.Should().Be(new DateTime(2024, 7, 15));
    }

    [Fact]
    public void GOOD_ActivatingSubscription_OnKnownDate_SetsCorrectExpiry()
    {
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Annual());

        sut.Activate(clock);

        sut.ExpiresAt.Should().Be(new DateTime(2025, 1, 1));
    }
}
