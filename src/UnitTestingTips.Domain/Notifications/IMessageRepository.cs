namespace UnitTestingTips.Domain.Notifications;

public interface IMessageRepository
{
    void Save(Message message);
    IReadOnlyList<Message> GetAll();
}
