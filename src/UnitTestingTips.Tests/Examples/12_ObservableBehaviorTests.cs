using FluentAssertions;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates testing observable behavior rather than implementation details.
///
/// Observable behavior = what the class exposes as its public contract
/// Implementation details = how the class achieves it internally
///
/// Tests should survive internal refactoring.
/// </summary>
public class ObservableBehaviorTests
{
    // ─────────────────────────────────────────────
    // GOOD: testing through observable behavior
    // ─────────────────────────────────────────────

    [Fact]
    public void Renewing_ASubscription_AllowsAccessForAnotherMonth()
    {
        var frozenTime = new DateTime(2024, 1, 1);
        var clock = new FixedClock(frozenTime);
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());
        sut.Activate(clock);

        // Extend from Jan 1 → Feb 1 (monthly plan)
        sut.RenewUntil(frozenTime.AddMonths(2)); // renew for another month (Feb 1 → Mar 1)

        // Verify through domain behavior — not internal state
        sut.IsActiveAt(new DateTime(2024, 2, 28)).Should().BeTrue();
        sut.IsActiveAt(new DateTime(2024, 3, 2)).Should().BeFalse();
    }

    [Fact]
    public void Deactivating_AnActiveSubscription_PreventsAccess()
    {
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());
        sut.Activate(clock);

        sut.Deactivate();

        // Observable behavior: deactivated subscription is not accessible
        sut.IsActiveAt(new DateTime(2024, 1, 15)).Should().BeFalse();
    }

    [Fact]
    public void ActiveSubscription_WithinValidPeriod_IsAccessible()
    {
        var activatedAt = new DateTime(2024, 1, 1);
        var clock = new FixedClock(activatedAt);
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());
        sut.Activate(clock);

        // Observable: can check access at a specific date
        sut.IsActiveAt(new DateTime(2024, 1, 15)).Should().BeTrue();
        sut.IsActiveAt(new DateTime(2024, 1, 31)).Should().BeTrue();
    }

    [Fact]
    public void ActiveSubscription_AfterExpiry_IsNotAccessible()
    {
        var activatedAt = new DateTime(2024, 1, 1);
        var clock = new FixedClock(activatedAt);
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());
        sut.Activate(clock);

        // Observable: cannot access after the plan expires
        sut.IsActiveAt(new DateTime(2024, 2, 2)).Should().BeFalse();
    }

    // ─────────────────────────────────────────────
    // AVOID: adding getters purely for tests
    // ─────────────────────────────────────────────

    // Don't do this in production code:
    //
    // public DateTime RenewalDate => _renewalDate; // Added only for testing
    //
    // Instead, expose behavior that clients actually need (IsActiveAt, CanAccess, etc.)
}
