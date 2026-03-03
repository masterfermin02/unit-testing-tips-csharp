namespace UnitTestingTips.Domain.Orders;

public record ReportLine(string Category, decimal Amount);

public class Report
{
    public decimal Total { get; }

    public Report(decimal total)
    {
        Total = total;
    }
}

public class ReportCalculator
{
    public Report Calculate(IEnumerable<ReportLine> lines)
    {
        return new Report(lines.Sum(l => l.Amount));
    }
}
