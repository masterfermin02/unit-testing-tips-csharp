using UnitTestingTips.Domain.Common;

namespace UnitTestingTips.Tests.Doubles;

public class FixedClock : IClock
{
    private readonly DateTime _now;

    public FixedClock(DateTime now) => _now = now;

    public DateTime UtcNow => _now;
}
