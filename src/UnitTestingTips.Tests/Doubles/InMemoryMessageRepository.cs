using UnitTestingTips.Domain.Notifications;

namespace UnitTestingTips.Tests.Doubles;

public class InMemoryMessageRepository : IMessageRepository
{
    private readonly List<Message> _messages = new();

    public void Save(Message message) => _messages.Add(message);

    public IReadOnlyList<Message> GetAll() => _messages.AsReadOnly();
}
