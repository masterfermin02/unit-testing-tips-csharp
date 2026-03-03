namespace UnitTestingTips.Domain.Notifications;

public interface IMailer
{
    void Send(Message message);
}
