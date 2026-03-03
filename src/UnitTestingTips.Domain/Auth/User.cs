namespace UnitTestingTips.Domain.Auth;

public class User
{
    private const int MinPasswordLength = 8;

    public Guid Id { get; }
    public string Email { get; }
    public string PasswordHash { get; }

    public User(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Invalid email address.", nameof(email));

        if (password.Length < MinPasswordLength)
            throw new ArgumentException(
                $"Password must be at least {MinPasswordLength} characters.", nameof(password));

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = BCryptHash(password);
    }

    private static string BCryptHash(string password) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password + "salt"));
}
