using UnitTestingTips.Domain.Notifications;

namespace UnitTestingTips.Tests.Doubles;

public class DummyMailer : IMailer
{
    public void Send(Message message)
    {
        // Intentionally empty — this dependency is required but unused in this test
    }
}
