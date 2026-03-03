using FluentAssertions;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates test naming best practices.
///
/// ❌ Poor names: Test(), TestDeactivate(), ItThrowsWhenInactive()
/// ✅ Good names: Describe behavior clearly using underscores and plain language.
///
/// Pattern: Subject_Scenario_ExpectedBehavior
/// </summary>
public class NamingTests
{
    // The variable holding the system under test should be named "sut"
    // Test names use underscores and describe observable business behavior

    [Fact]
    public void Deactivating_AnActiveSubscription_Succeeds()
    {
        var sut = SubscriptionMother.Active();

        sut.Deactivate();

        sut.Status.Should().Be(SubscriptionStatus.Inactive);
    }

    [Fact]
    public void Deactivating_AnInactiveSubscription_ThrowsInvalidOperation()
    {
        var sut = SubscriptionMother.Inactive();

        var act = () => sut.Deactivate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Creating_ANewSubscription_HasNewStatus()
    {
        var sut = SubscriptionMother.New();

        sut.Status.Should().Be(SubscriptionStatus.New);
    }

    // Compare poor naming vs. good naming:

    // ❌ Poor: doesn't say what the behavior is
    [Fact]
    public void POOR_NAME_TestDeactivate()
    {
        var sub = SubscriptionMother.Active();
        sub.Deactivate();
        sub.Status.Should().Be(SubscriptionStatus.Inactive);
    }

    // ✅ Good: describes what should happen in plain language
    [Fact]
    public void GOOD_NAME_Deactivating_ActiveSubscription_ChangesStatusToInactive()
    {
        var sut = SubscriptionMother.Active();
        sut.Deactivate();
        sut.Status.Should().Be(SubscriptionStatus.Inactive);
    }
}
