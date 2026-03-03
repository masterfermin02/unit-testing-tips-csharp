using FluentAssertions;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the Arrange-Act-Assert (AAA) pattern.
///
/// Every test has exactly three sections:
///   Arrange: set up the SUT and its dependencies
///   Act:     invoke the behavior (ideally one line)
///   Assert:  verify the observable outcome
/// </summary>
public class AAAPatternTests
{
    [Fact]
    public void Deactivating_AnActiveSubscription_ChangesStatusToInactive()
    {
        // Arrange
        var sut = SubscriptionMother.Active();

        // Act
        sut.Deactivate();

        // Assert
        sut.Status.Should().Be(SubscriptionStatus.Inactive);
    }

    [Fact]
    public void Activating_ANewSubscription_SetsActiveStatus()
    {
        // Arrange
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());

        // Act
        sut.Activate(clock);

        // Assert
        sut.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Activating_ANewSubscription_SetsExpiryOneMonthAhead()
    {
        // Arrange
        var frozenTime = new DateTime(2024, 3, 15);
        var clock = new FixedClock(frozenTime);
        var sut = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());

        // Act
        sut.Activate(clock);

        // Assert — each test covers one behavior
        sut.ExpiresAt.Should().Be(new DateTime(2024, 4, 15));
    }

    // ❌ Avoid multiple act-assert cycles in one test
    [Fact]
    public void AVOID_MultipleActsInOneTest()
    {
        var clock = new FixedClock(DateTime.UtcNow);
        var sub = new Subscription(SubscriptionStatus.New, SubscriptionPlan.Monthly());

        sub.Activate(clock);
        sub.Status.Should().Be(SubscriptionStatus.Active); // assert 1

        sub.Deactivate();
        sub.Status.Should().Be(SubscriptionStatus.Inactive); // assert 2 — this is a second test!
    }
}
