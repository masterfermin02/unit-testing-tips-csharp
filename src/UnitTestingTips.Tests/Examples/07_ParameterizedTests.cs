using FluentAssertions;
using UnitTestingTips.Domain.Auth;
using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Orders;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates parameterized tests using xUnit's [Theory] and [InlineData]/[MemberData].
///
/// Best practices:
/// - Separate positive and negative test cases into different theories
/// - Use [MemberData] for complex objects
/// - Name each theory clearly — it should read as a complete sentence
/// </summary>
public class ParameterizedTests
{
    // ─────────────────────────────────────────────
    // SEPARATE positive and negative cases
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData("validpassword")]
    [InlineData("another_valid_pass!")]
    [InlineData("LongEnoughPassword1")]
    [InlineData("12345678")]
    public void Creating_User_WithValidPassword_Succeeds(string password)
    {
        var act = () => new User("user@example.com", password);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("1234567")]
    [InlineData("abc")]
    public void Creating_User_WithTooShortPassword_ThrowsArgumentException(string password)
    {
        var act = () => new User("user@example.com", password);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    // ─────────────────────────────────────────────
    // NOT RECOMMENDED: mixing success and failure in one theory
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData("validpassword", true)]
    [InlineData("short", false)]
    public void NOT_RECOMMENDED_ValidatingPassword_MixesSuccessAndFailure(string password, bool shouldSucceed)
    {
        // Mixing positive/negative in one theory makes failures harder to diagnose
        if (shouldSucceed)
        {
            var act = () => new User("user@example.com", password);
            act.Should().NotThrow();
        }
        else
        {
            var act = () => new User("user@example.com", password);
            act.Should().Throw<ArgumentException>();
        }
    }

    // ─────────────────────────────────────────────
    // MemberData for complex scenarios
    // ─────────────────────────────────────────────

    public static IEnumerable<object[]> DiscountCalculationData => new[]
    {
        new object[] { CustomerTier.Standard, 50m, 0m },        // < 100 threshold, no discount
        new object[] { CustomerTier.Standard, 100m, 5m },       // exactly at threshold
        new object[] { CustomerTier.Standard, 200m, 10m },      // above threshold
        new object[] { CustomerTier.Premium, 100m, 20m },       // premium: 20%
        new object[] { CustomerTier.Enterprise, 100m, 30m },    // enterprise: 30%
    };

    [Theory]
    [MemberData(nameof(DiscountCalculationData))]
    public void CalculatingDiscount_ReturnsCorrectAmount(
        CustomerTier tier, decimal orderAmount, decimal expectedDiscount)
    {
        var sut = new DiscountCalculator();

        var discount = sut.Calculate(tier, orderAmount);

        discount.Should().Be(expectedDiscount);
    }

    // ─────────────────────────────────────────────
    // MemberData with reason/description
    // ─────────────────────────────────────────────

    public static IEnumerable<object[]> InvalidDiscountPercentData => new[]
    {
        new object[] { -1m },
        new object[] { 0m },
        new object[] { 101m },
        new object[] { 200m },
    };

    [Theory]
    [MemberData(nameof(InvalidDiscountPercentData))]
    public void ApplyingDiscount_WithInvalidPercent_ThrowsArgumentException(decimal invalidDiscount)
    {
        var order = new OrderBuilder().WithItem("Widget", 100m).Build();

        var act = () => order.ApplyDiscount(invalidDiscount);

        act.Should().Throw<ArgumentException>();
    }
}

// Minimal OrderBuilder for this file's tests (full one is in Builders folder)
file class OrderBuilder
{
    private readonly List<UnitTestingTips.Domain.Orders.OrderItem> _items = new();

    public OrderBuilder WithItem(string name, decimal price)
    {
        _items.Add(new UnitTestingTips.Domain.Orders.OrderItem(name, new Money(price)));
        return this;
    }

    public UnitTestingTips.Domain.Orders.Order Build() =>
        new(UnitTestingTips.Domain.Customers.CustomerId.New(), DateTime.UtcNow, _items);
}
