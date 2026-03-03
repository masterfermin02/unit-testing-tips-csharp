using FluentAssertions;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates testing units of BEHAVIOR rather than units of CODE.
///
/// Anti-pattern: 1 test per method (unit = method)
///   → Fragile: breaks on any internal refactoring
///   → Low value: tests structure, not behavior
///
/// Recommended: tests describe business scenarios (unit = behavior)
///   → Survives internal refactoring
///   → Documents business rules clearly
/// </summary>
public class UnitOfBehaviorTests
{
    // ─────────────────────────────────────────────
    // Each test describes a BUSINESS SCENARIO
    // not "does SetStatus work"
    // ─────────────────────────────────────────────

    [Fact]
    public void Purchasing_MonthlySub_StartsAsActiveSubscription()
    {
        var customer = CustomerMother.Active();
        var plan = SubscriptionPlan.Monthly();
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var repo = new InMemorySubscriptionRepository();
        var sut = new SubscriptionService(repo, clock);

        var subscription = sut.Purchase(customer, plan);

        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Purchasing_MonthlySub_SetsRenewalDateOneMonthAhead()
    {
        var customer = CustomerMother.Active();
        var plan = SubscriptionPlan.Monthly();
        var frozenTime = new DateTime(2024, 1, 15);
        var clock = new FixedClock(frozenTime);
        var repo = new InMemorySubscriptionRepository();
        var sut = new SubscriptionService(repo, clock);

        var subscription = sut.Purchase(customer, plan);

        subscription.ExpiresAt.Should().Be(new DateTime(2024, 2, 15));
    }

    [Fact]
    public void Purchasing_AnnualSub_SetsRenewalDateTwelveMonthsAhead()
    {
        var customer = CustomerMother.Active();
        var plan = SubscriptionPlan.Annual();
        var frozenTime = new DateTime(2024, 1, 15);
        var clock = new FixedClock(frozenTime);
        var repo = new InMemorySubscriptionRepository();
        var sut = new SubscriptionService(repo, clock);

        var subscription = sut.Purchase(customer, plan);

        subscription.ExpiresAt.Should().Be(new DateTime(2025, 1, 15));
    }

    [Fact]
    public void Deactivating_AnActiveSubscription_PreventsRenewal()
    {
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var sut = SubscriptionMother.Inactive();

        // Deactivated subscriptions cannot be renewed (only active ones can)
        var act = () => sut.RenewUntil(new DateTime(2024, 3, 1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reactivating_AnInactiveSubscription_AllowsAccess()
    {
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var sut = SubscriptionMother.Inactive();

        sut.Reactivate(clock);

        sut.Status.Should().Be(SubscriptionStatus.Active);
    }

    // Compare this test organization to a 1:1 method mapping:
    //
    // ❌ Poor (method-centric):
    //   void SetStatus_Works() { }
    //   void SetRenewalDate_Works() { }
    //   void SetPlan_Works() { }
    //
    // ✅ Good (behavior-centric, above):
    //   void Purchasing_MonthlySub_StartsAsActiveSubscription() { }
    //   void Purchasing_MonthlySub_SetsRenewalDateOneMonthAhead() { }
    //   void Deactivating_AnActiveSubscription_PreventsRenewal() { }
}
