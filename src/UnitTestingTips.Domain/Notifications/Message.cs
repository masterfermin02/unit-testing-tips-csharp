namespace UnitTestingTips.Domain.Notifications;

public class Message
{
    public string To { get; }
    public string Subject { get; }
    public string Body { get; }

    public Message(string to, string subject, string body = "")
    {
        To = to;
        Subject = subject;
        Body = body;
    }

    public override bool Equals(object? obj) =>
        obj is Message other && To == other.To && Subject == other.Subject;

    public override int GetHashCode() => HashCode.Combine(To, Subject);
}
