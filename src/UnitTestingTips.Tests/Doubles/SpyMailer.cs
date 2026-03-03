using UnitTestingTips.Domain.Notifications;

namespace UnitTestingTips.Tests.Doubles;

public class SpyMailer : IMailer
{
    public List<Message> SentMessages { get; } = new();

    public void Send(Message message)
    {
        SentMessages.Add(message);
    }

    public int SentCount => SentMessages.Count;

    public bool WasSentTo(string email) =>
        SentMessages.Any(m => m.To == email);
}
