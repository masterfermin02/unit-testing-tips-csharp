using UnitTestingTips.Domain.Notifications;

namespace UnitTestingTips.Domain.Auth;

public class UserRegistrationService
{
    private readonly IUniqueEmailSpecification _uniqueEmail;
    private readonly IMailer _mailer;

    public UserRegistrationService(IUniqueEmailSpecification uniqueEmail, IMailer mailer)
    {
        _uniqueEmail = uniqueEmail;
        _mailer = mailer;
    }

    public User Register(string email, string password)
    {
        if (!_uniqueEmail.IsUnique(email))
            throw new InvalidOperationException($"Email '{email}' is already in use.");

        var user = new User(email, password);

        _mailer.Send(new Message(email, "Welcome!", "Thanks for registering."));

        return user;
    }
}
