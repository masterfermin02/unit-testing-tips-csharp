using FluentAssertions;
using Moq;
using UnitTestingTips.Domain.Auth;
using UnitTestingTips.Domain.Customers;
using UnitTestingTips.Tests.Doubles;
using UnitTestingTips.Tests.Mothers;
using Xunit;

namespace UnitTestingTips.Tests.Examples;

/// <summary>
/// Demonstrates the correct use of stubs vs. mocks.
///
/// Stub = controls INPUT into the SUT (what the SUT receives from collaborators)
/// Mock = verifies OUTPUT from the SUT (what the SUT sends to collaborators)
///
///   [Collaborator] --stub--> [SUT] --mock--> [Collaborator]
///      (incoming)                              (outgoing)
///
/// Rule: Never assert on stub interactions.
/// </summary>
public class MockVsStubTests
{
    // ─────────────────────────────────────────────
    // STUB: controls input into SUT
    // ─────────────────────────────────────────────

    [Fact]
    public void Registering_WhenEmailIsAlreadyTaken_ThrowsException()
    {
        // NeverUniqueEmailStub STUBS the input: forces the SUT to see a taken email
        var emailSpec = new NeverUniqueEmailStub();
        var mailer = new DummyMailer();
        var sut = new UserRegistrationService(emailSpec, mailer);

        var act = () => sut.Register("taken@example.com", "securepassword123");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already in use*");
    }

    [Fact]
    public void Registering_WhenEmailIsUnique_Succeeds()
    {
        // AlwaysUniqueEmailStub STUBS the input: forces SUT to see an available email
        var emailSpec = new AlwaysUniqueEmailStub();
        var mailer = new DummyMailer();
        var sut = new UserRegistrationService(emailSpec, mailer);

        var act = () => sut.Register("new@example.com", "securepassword123");

        act.Should().NotThrow();
    }

    // ─────────────────────────────────────────────
    // MOCK/SPY: verifies output from SUT
    // ─────────────────────────────────────────────

    [Fact]
    public void Registering_NewUser_SendsWelcomeEmail()
    {
        var emailSpec = new AlwaysUniqueEmailStub(); // stub (input control)
        var mailer = new SpyMailer();                // spy (output verification)
        var sut = new UserRegistrationService(emailSpec, mailer);

        sut.Register("new@example.com", "securepassword123");

        // Assert on the outgoing interaction (side effect)
        mailer.WasSentTo("new@example.com").Should().BeTrue();
    }

    [Fact]
    public void Registering_NewUser_SendsExactlyOneEmail()
    {
        var emailSpec = new AlwaysUniqueEmailStub();
        var mailer = new SpyMailer();
        var sut = new UserRegistrationService(emailSpec, mailer);

        sut.Register("new@example.com", "securepassword123");

        mailer.SentCount.Should().Be(1);
    }

    // ─────────────────────────────────────────────
    // WRONG: asserting on stub interactions
    // ─────────────────────────────────────────────

    [Fact]
    public void WRONG_Asserting_OnStub_IsFragile()
    {
        var mockEmailSpec = new Mock<IUniqueEmailSpecification>();
        mockEmailSpec.Setup(s => s.IsUnique(It.IsAny<string>())).Returns(true);

        var mailer = new DummyMailer();
        var sut = new UserRegistrationService(mockEmailSpec.Object, mailer);
        sut.Register("new@example.com", "securepassword123");

        // ❌ Wrong: the email spec is a stub (it provides INPUT), not an output.
        // Asserting on it couples the test to implementation details.
        mockEmailSpec.Verify(s => s.IsUnique("new@example.com"), Times.Once);
    }

    // ─────────────────────────────────────────────
    // CUSTOMER SERVICE: stub for repository lookup
    // ─────────────────────────────────────────────

    [Fact]
    public void GettingCustomer_WhenExists_ReturnsCorrectCustomer()
    {
        // Stub (fake repository) controls what the SUT receives
        var existingCustomer = CustomerMother.Active();
        var stub = new InMemoryCustomerRepository();
        stub.Store(existingCustomer);

        var sut = new CustomerService(stub);

        var result = sut.Get(existingCustomer.Id.Value);

        result.Should().BeEquivalentTo(existingCustomer);
    }

    [Fact]
    public void GettingCustomer_WhenNotExists_ThrowsNotFoundException()
    {
        var emptyRepo = new InMemoryCustomerRepository();
        var sut = new CustomerService(emptyRepo);

        var act = () => sut.Get(Guid.NewGuid());

        act.Should().Throw<CustomerNotFoundException>();
    }
}
