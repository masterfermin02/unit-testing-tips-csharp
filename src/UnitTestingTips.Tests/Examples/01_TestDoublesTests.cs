using FluentAssertions;
using Moq;
using UnitTestingTips.Domain.Notifications;
using UnitTestingTips.Tests.Doubles;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the five types of test doubles:
/// Dummy, Fake, Stub, Spy, Mock
/// and when to prefer fakes/spies over mocks.
/// </summary>
public class TestDoublesTests
{
    // ─────────────────────────────────────────────
    // DUMMY: satisfies a dependency but is never called
    // ─────────────────────────────────────────────

    [Fact]
    public void NotificationService_WithDummyMailer_WhenNoMessages_SendsNothing()
    {
        var repository = new InMemoryMessageRepository(); // empty — no messages
        var mailer = new DummyMailer();                   // dummy: won't be called
        var sut = new NotificationService(mailer, repository);

        // This test verifies the service handles zero messages without error.
        // The DummyMailer satisfies the interface but will never be invoked.
        var act = () => sut.Send();

        act.Should().NotThrow();
    }

    // ─────────────────────────────────────────────
    // FAKE: working simplified implementation
    // ─────────────────────────────────────────────

    [Fact]
    public void InMemoryMessageRepository_StoresAndRetrievesMessages()
    {
        var fake = new InMemoryMessageRepository();
        var msg = new Message("user@example.com", "Hello");

        fake.Save(msg);
        var all = fake.GetAll();

        all.Should().ContainSingle().Which.Should().Be(msg);
    }

    // ─────────────────────────────────────────────
    // SPY: records interactions for later assertion
    // ─────────────────────────────────────────────

    [Fact]
    public void NotificationService_WithMessages_SendsEachMessage()
    {
        var message1 = new Message("a@example.com", "Hello A");
        var message2 = new Message("b@example.com", "Hello B");

        var repository = new InMemoryMessageRepository();
        repository.Save(message1);
        repository.Save(message2);

        var mailer = new SpyMailer();
        var sut = new NotificationService(mailer, repository);

        sut.Send();

        mailer.SentMessages.Should().BeEquivalentTo(new[] { message1, message2 });
    }

    [Fact]
    public void SpyMailer_TracksRecipients()
    {
        var mailer = new SpyMailer();
        mailer.Send(new Message("alice@example.com", "Hi"));

        mailer.WasSentTo("alice@example.com").Should().BeTrue();
        mailer.WasSentTo("bob@example.com").Should().BeFalse();
    }

    // ─────────────────────────────────────────────
    // NOT RECOMMENDED: over-use of mocks
    // ─────────────────────────────────────────────

    [Fact]
    public void NOT_RECOMMENDED_Sends_AllNotifications_WithMocks()
    {
        var message1 = new Message("a@example.com", "Hello A");
        var message2 = new Message("b@example.com", "Hello B");

        // Mocking inputs (repo) AND outputs (mailer) — tightly coupled to implementation
        var repo = new Mock<IMessageRepository>();
        repo.Setup(x => x.GetAll()).Returns(new[] { message1, message2 });

        var mailer = new Mock<IMailer>();

        var sut = new NotificationService(mailer.Object, repo.Object);
        sut.Send();

        mailer.Verify(x => x.Send(message1), Times.Once);
        mailer.Verify(x => x.Send(message2), Times.Once);
    }

    // ─────────────────────────────────────────────
    // RECOMMENDED: fakes for input, spy for output
    // ─────────────────────────────────────────────

    [Fact]
    public void RECOMMENDED_Sends_AllNotifications_WithFakesAndSpies()
    {
        var message1 = new Message("a@example.com", "Hello A");
        var message2 = new Message("b@example.com", "Hello B");

        var repository = new InMemoryMessageRepository();
        repository.Save(message1);
        repository.Save(message2);

        var mailer = new SpyMailer();
        var sut = new NotificationService(mailer, repository);

        sut.Send();

        mailer.SentMessages.Should().BeEquivalentTo(new[] { message1, message2 });
    }
}
