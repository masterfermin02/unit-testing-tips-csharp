using FluentAssertions;
using Moq;
using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Notifications;
using UnitTestingTips.Domain.Orders;
using UnitTestingTips.Tests.Builders;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the three testing styles, ranked from best to worst:
///
/// 1. OUTPUT (best)  — verify return value / result
///    Highest refactoring resistance, lowest maintenance cost.
///
/// 2. STATE (middle) — verify object state after operation
///    Good when no return value, slightly more coupled to internals.
///
/// 3. COMMUNICATION (worst) — verify method calls between objects
///    Lowest refactoring resistance, highest maintenance cost.
///    Use sparingly, only for true outgoing side-effect commands.
/// </summary>
public class TestingStylesTests
{
    // ─────────────────────────────────────────────
    // 1. OUTPUT TESTING — Best
    // ─────────────────────────────────────────────

    [Fact]
    public void OUTPUT_CalculatingDiscount_ForPremiumCustomer_Returns20Percent()
    {
        var sut = new DiscountCalculator();

        var discount = sut.Calculate(CustomerTier.Premium, orderAmount: 100m);

        // Tests the RETURN VALUE — no knowledge of internal implementation needed
        discount.Should().Be(20m);
    }

    [Fact]
    public void OUTPUT_PlacingValidOrder_ReturnsSuccessResult()
    {
        var sut = new OrderService();
        var command = new PlaceOrderCommand(
            CustomerId.New(),
            new[] { new OrderItemDto("Widget", 19.99m) }
        );

        var result = sut.Place(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void OUTPUT_PlacingEmptyOrder_ReturnsFailureResult()
    {
        var sut = new OrderService();
        var command = new PlaceOrderCommand(CustomerId.New(), Array.Empty<OrderItemDto>());

        var result = sut.Place(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("item");
    }

    // ─────────────────────────────────────────────
    // 2. STATE TESTING — Middle
    // ─────────────────────────────────────────────

    [Fact]
    public void STATE_DeactivatingSubscription_ChangesStatusToInactive()
    {
        var sut = SubscriptionMother.Active();

        sut.Deactivate();

        // Tests the STATE change — necessary here since Deactivate() is void
        sut.Status.Should().Be(UnitTestingTips.Domain.Subscriptions.SubscriptionStatus.Inactive);
    }

    [Fact]
    public void STATE_AddingItemToOrder_IncreasesTotal()
    {
        var order = new OrderBuilder()
            .WithItem("Widget", 10m)
            .Build();

        // State testing: verify the resulting state of the object
        order.Total.Amount.Should().Be(10m);
        order.Items.Should().HaveCount(1);
    }

    // ─────────────────────────────────────────────
    // 3. COMMUNICATION TESTING — Worst (use sparingly)
    // ─────────────────────────────────────────────

    [Fact]
    public void COMMUNICATION_SendingNotifications_CallsMailerForEachMessage()
    {
        var message = new Message("user@example.com", "Hello");
        var repo = new InMemoryMessageRepository();
        repo.Save(message);

        // Communication testing: verifying interaction via mock
        var mockMailer = new Mock<IMailer>();
        var sut = new NotificationService(mockMailer.Object, repo);

        sut.Send();

        // This is fragile: if we rename Send() to Deliver(), this test breaks
        // even though the behavior (notification sent) is unchanged
        mockMailer.Verify(m => m.Send(message), Times.Once);
    }

    // Better alternative for the above: use Spy (still communication, but less coupled to framework)
    [Fact]
    public void COMMUNICATION_BETTER_SendingNotifications_DeliverMessagesToMailer()
    {
        var message = new Message("user@example.com", "Hello");
        var repo = new InMemoryMessageRepository();
        repo.Save(message);

        var mailer = new SpyMailer(); // spy is less coupled than Mock<T>.Verify
        var sut = new NotificationService(mailer, repo);

        sut.Send();

        mailer.SentMessages.Should().ContainSingle().Which.Should().Be(message);
    }
}
