namespace UnitTestingTips.Domain.Notifications;

public class NotificationService
{
    private readonly IMailer _mailer;
    private readonly IMessageRepository _repository;

    public NotificationService(IMailer mailer, IMessageRepository repository)
    {
        _mailer = mailer;
        _repository = repository;
    }

    public void Send()
    {
        var messages = _repository.GetAll();
        foreach (var message in messages)
        {
            _mailer.Send(message);
        }
    }
}
