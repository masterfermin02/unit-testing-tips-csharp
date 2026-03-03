using FluentAssertions;
using Moq;
using UnitTestingTips.Domain.Subscriptions;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the two schools of unit testing.
///
/// Classical (Detroit) School:
///   Unit = unit of BEHAVIOR (can span multiple real classes)
///   Collaborators are real objects or fakes
///   Higher refactoring resistance, lower fragility
///
/// Mockist (London) School:
///   Unit = single CLASS
///   All collaborators replaced with mocks
///   Higher fragility, lower refactoring resistance
///
/// Recommendation: prefer the Classical school.
/// </summary>
public class TwoSchoolsTests
{
    // ─────────────────────────────────────────────
    // Classical School: real collaborators / fakes
    // ─────────────────────────────────────────────

    [Fact]
    public void CLASSICAL_Purchasing_ASubscriptionPlan_StartsActiveSubscription()
    {
        // Arrange — real classes collaborate
        var customer = CustomerMother.Active();
        var plan = SubscriptionPlan.Monthly();
        var repository = new InMemorySubscriptionRepository();
        var clock = new FixedClock(new DateTime(2024, 1, 1));
        var sut = new SubscriptionService(repository, clock);

        // Act
        var subscription = sut.Purchase(customer, plan);

        // Assert — verify observable result, not interactions
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        repository.Count.Should().Be(1);
    }

    [Fact]
    public void CLASSICAL_Purchasing_AMonthlyPlan_SetsExpiryOneMonthAhead()
    {
        var customer = CustomerMother.Active();
        var plan = SubscriptionPlan.Monthly();
        var frozenTime = new DateTime(2024, 3, 15);
        var repository = new InMemorySubscriptionRepository();
        var clock = new FixedClock(frozenTime);
        var sut = new SubscriptionService(repository, clock);

        var subscription = sut.Purchase(customer, plan);

        subscription.ExpiresAt.Should().Be(new DateTime(2024, 4, 15));
    }

    // ─────────────────────────────────────────────
    // Mockist School: everything mocked
    // ─────────────────────────────────────────────

    [Fact]
    public void MOCKIST_Purchasing_ASubscriptionPlan_CallsSaveOnRepository()
    {
        // All collaborators are mocked — tests the interaction, not the outcome
        var customer = CustomerMother.Active();
        var plan = SubscriptionPlan.Monthly();
        var mockRepository = new Mock<ISubscriptionRepository>();
        var clock = new FixedClock(new DateTime(2024, 1, 1));

        var sut = new SubscriptionService(mockRepository.Object, clock);
        sut.Purchase(customer, plan);

        // Verifies the call was made — fragile if implementation changes
        mockRepository.Verify(r => r.Save(It.IsAny<Subscription>()), Times.Once);
    }

    // The Mockist test above is fragile:
    // If we refactor SubscriptionService to batch-save, or rename Save to Persist,
    // the test breaks even though behavior is unchanged.
    //
    // The Classical tests above survive such refactoring.
}
