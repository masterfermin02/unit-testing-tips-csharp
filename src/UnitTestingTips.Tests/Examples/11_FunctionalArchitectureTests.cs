using FluentAssertions;
using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Domain.Orders;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates functional architecture: separating pure logic from side effects.
///
/// Pure logic (no side effects) → unit tests: fast, deterministic, no mocking
/// Infrastructure (side effects) → integration tests
/// Orchestration (thin shell) → integration tests
///
/// Architecture:
///   [FileLoader] ──► [ApplicationService] ◄─── [Calculator]
///   (side effects)    (thin orchestration)        (pure logic)
///    integration tests                            unit tests ← this file
/// </summary>
public class FunctionalArchitectureTests
{
    // ─────────────────────────────────────────────
    // UNIT TESTS for pure logic only
    // Pure functions: same input → same output, no side effects
    // ─────────────────────────────────────────────

    [Fact]
    public void Calculating_ReportWithPositiveLines_SumsTotals()
    {
        var lines = new[]
        {
            new ReportLine("Sales", 1000m),
            new ReportLine("Bonuses", 200m),
        };

        var sut = new ReportCalculator();
        var report = sut.Calculate(lines);

        report.Total.Should().Be(1200m);
    }

    [Fact]
    public void Calculating_ReportWithNegativeLines_IncludesDeductions()
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

    [Fact]
    public void Calculating_EmptyReport_ReturnsZeroTotal()
    {
        var sut = new ReportCalculator();
        var report = sut.Calculate(Enumerable.Empty<ReportLine>());

        report.Total.Should().Be(0m);
    }

    [Fact]
    public void Calculating_SingleLine_ReturnsLineAmount()
    {
        var lines = new[] { new ReportLine("Category", 42.50m) };

        var sut = new ReportCalculator();
        var report = sut.Calculate(lines);

        report.Total.Should().Be(42.50m);
    }

    // ─────────────────────────────────────────────
    // DISCOUNT CALCULATOR: pure function, trivially testable
    // ─────────────────────────────────────────────

    // Note: decimal is not allowed in [InlineData] (CLR attribute limitation).
    // Use MemberData for scenarios with decimal values.
    public static IEnumerable<object[]> DiscountData => new[]
    {
        new object[] { CustomerTier.Standard, 50.0,  0.0  },   // below threshold
        new object[] { CustomerTier.Standard, 100.0, 5.0  },   // at threshold: 5%
        new object[] { CustomerTier.Premium,  100.0, 20.0 },   // premium: 20%
        new object[] { CustomerTier.Enterprise, 100.0, 30.0 }, // enterprise: 30%
    };

    [Theory]
    [MemberData(nameof(DiscountData))]
    public void DiscountCalculator_ReturnsCorrectDiscount(
        CustomerTier tier, double amount, double expected)
    {
        var sut = new DiscountCalculator();

        var discount = sut.Calculate(tier, (decimal)amount);

        discount.Should().Be((decimal)expected);
    }
}
