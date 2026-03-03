using FluentAssertions;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the Object Mother pattern.
///
/// Object Mother: a factory class with named methods that create
/// pre-configured test objects in specific, well-named states.
///
/// Benefits:
/// - One place to update when constructors change
/// - Names communicate intent (Active() vs. Inactive())
/// - Reusable across many test classes
/// </summary>
public class ObjectMotherTests
{
    [Fact]
    public void Deactivating_AnActiveSubscription_Succeeds()
    {
        // SubscriptionMother.Active() creates a subscription in the Active state
        var sut = SubscriptionMother.Active();

        sut.Deactivate();

        sut.Status.Should().Be(SubscriptionStatus.Inactive);
    }

    [Fact]
    public void Deactivating_AnInactiveSubscription_ThrowsException()
    {
        // SubscriptionMother.Inactive() creates a subscription in the Inactive state
        var sut = SubscriptionMother.Inactive();

        var act = () => sut.Deactivate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active*");
    }

    [Fact]
    public void NewSubscription_HasNewStatus()
    {
        var sut = SubscriptionMother.New();

        sut.Status.Should().Be(SubscriptionStatus.New);
    }

    [Fact]
    public void ActiveSubscription_WithAnnualPlan_HasLongerExpiry()
    {
        var sut = SubscriptionMother.ActiveWithPlan(SubscriptionPlan.Annual());

        // Annual plan = 12 months duration — check it's set
        sut.ExpiresAt.Should().NotBeNull();
        sut.Status.Should().Be(SubscriptionStatus.Active);
    }
}
