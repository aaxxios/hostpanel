using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Support.Commands.SendContactMessage;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// The public contact form's <c>POST /api/contact</c> route, and
/// <see cref="SendContactMessageHandler"/> exercised directly against fakes of the two ports it
/// depends on -- the SMTP relay and the operator's chat -- neither of which this test host can
/// reach for real. This is the shape the code review guidance recommends for a port this project
/// does not own: a fake, not a mock of our own code, and the handler itself is real.
/// </summary>
/// <remarks>
/// Merged from the deleted <c>ContactEndpointTests</c> and <c>SendContactMessageHandlerTests</c>.
/// The HTTP tests cover the refusal the default test configuration reaches on its own
/// (<c>CONTACT_NOT_CONFIGURED</c>) and the validator; the handler tests cover the two outcomes
/// that need a relay to behave in a specific way (accept, or refuse) that no test-tier
/// configuration can produce. <c>ContactController</c> is <c>[AllowAnonymous]</c>, so there is no
/// authorization boundary to prove here.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SendContactMessageTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The enquiry inbox address used by every handler-level test in this class.</summary>
    private const string ContactEmail = "enquiries@hostpanel.test";

    /// <summary>A well-formed submission, reused by every handler-level test in this class.</summary>
    private static readonly SendContactMessageCommand Submission = new(
        Name: "Jane Visitor",
        Email: "jane@example.com",
        Phone: null,
        Service: null,
        Message: "I would like a quote.",
        SubmittedAt: null);

    /// <summary>Reads the problem+json body's <c>type</c> (error code) field.</summary>
    /// <param name="response">The refused response.</param>
    /// <returns>The error code.</returns>
    private static async Task<string> ReadCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("type").GetString()!;
    }

    /// <summary>Builds the handler under test with an enquiry inbox configured.</summary>
    /// <param name="emailSender">The fake SMTP relay.</param>
    /// <param name="contactNotifier">The fake chat notifier.</param>
    /// <returns>The handler.</returns>
    private static SendContactMessageHandler BuildHandler(IEmailSender emailSender, IContactNotifier contactNotifier)
    {
        var options = Options.Create(new NotificationOptions { AdminEmail = ContactEmail, ContactEmail = ContactEmail });
        return new SendContactMessageHandler(
            emailSender, contactNotifier, options, NullLogger<SendContactMessageHandler>.Instance);
    }

    /// <summary>
    /// A well-formed submission on a deployment with no configured enquiry inbox answers 503 with
    /// <c>CONTACT_NOT_CONFIGURED</c>, never a silent 204 -- the test host configures no
    /// <c>Notifications:ContactEmail</c>, so this is the refusal every submission over HTTP reaches.
    /// </summary>
    [Fact]
    public async Task RefusesWithNotConfiguredWhenNoEnquiryInboxIsSetAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", new
        {
            name = "Jane Visitor",
            email = "jane@example.com",
            phone = (string?)null,
            service = (string?)null,
            message = "I would like a quote.",
            submittedAt = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await ReadCodeAsync(response)).Should().Be(HostpanelErrorCodes.CONTACT_NOT_CONFIGURED);
    }

    /// <summary>A submission missing a required field is refused before the handler ever runs.</summary>
    [Fact]
    public async Task RefusesAMissingRequiredFieldAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", new
        {
            name = "",
            email = "jane@example.com",
            phone = (string?)null,
            service = (string?)null,
            message = "I would like a quote.",
            submittedAt = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>A malformed email address is refused before the handler ever runs.</summary>
    [Fact]
    public async Task RefusesAMalformedEmailAddressAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", new
        {
            name = "Jane Visitor",
            email = "not-an-email",
            phone = (string?)null,
            service = (string?)null,
            message = "I would like a quote.",
            submittedAt = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>A message the relay accepts is delivered, and the chat is notified afterwards.</summary>
    [Fact]
    public async Task AcceptedMessage_SucceedsAndNotifiesTheChatAsync()
    {
        var emailSender = new FakeEmailSender();
        var contactNotifier = new FakeContactNotifier();
        var handler = BuildHandler(emailSender, contactNotifier);

        var outcome = await handler.HandleAsync(Submission, CancellationToken.None);

        Assert.True(outcome.IsSuccess);
        Assert.Single(emailSender.Sent);
        Assert.Equal(ContactEmail, emailSender.Sent[0].To);
        Assert.Single(contactNotifier.Notified);
    }

    /// <summary>
    /// A relay that refuses the message answers <c>CONTACT_SEND_FAILED</c>, and the chat is never
    /// notified about a message that was not actually delivered.
    /// </summary>
    [Fact]
    public async Task RelayRefusal_AnswersContactSendFailedAndSkipsTheChatAsync()
    {
        var emailSender = new FakeEmailSender { ThrowOnSend = true };
        var contactNotifier = new FakeContactNotifier();
        var handler = BuildHandler(emailSender, contactNotifier);

        var outcome = await handler.HandleAsync(Submission, CancellationToken.None);

        Assert.True(outcome.IsFailure);
        Assert.Equal(HostpanelErrorCodes.CONTACT_SEND_FAILED, outcome.ErrorCode);
        Assert.Empty(contactNotifier.Notified);
    }

    /// <summary>
    /// The chat channel failing does not turn a delivered mail into a refusal: the mail is the
    /// record the operator's process is built on, and a Telegram outage must not report it lost.
    /// </summary>
    [Fact]
    public async Task ChatOutage_DoesNotFailTheOverallSubmissionAsync()
    {
        var emailSender = new FakeEmailSender();
        var contactNotifier = new FakeContactNotifier { ThrowOnNotify = true };
        var handler = BuildHandler(emailSender, contactNotifier);

        var outcome = await handler.HandleAsync(Submission, CancellationToken.None);

        Assert.True(outcome.IsSuccess);
        Assert.Single(emailSender.Sent);
    }

    /// <summary>A fake SMTP relay: records what it was asked to send, and can be made to refuse.</summary>
    private sealed class FakeEmailSender : IEmailSender
    {
        /// <summary>Every message the handler asked this fake to send.</summary>
        public List<EmailMessage> Sent { get; } = [];

        /// <summary>When set, every call throws instead of recording the message.</summary>
        public bool ThrowOnSend { get; init; }

        /// <inheritdoc/>
        public Task SendAsync(EmailMessage message, CancellationToken ct)
        {
            if (ThrowOnSend)
            {
                throw new InvalidOperationException("The fake relay was told to refuse this message.");
            }

            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>A fake chat channel: records what it was asked to post, and can be made to fail.</summary>
    private sealed class FakeContactNotifier : IContactNotifier
    {
        /// <summary>Every enquiry this fake was asked to post.</summary>
        public List<ContactEnquiry> Notified { get; } = [];

        /// <summary>When set, every call throws instead of recording the enquiry.</summary>
        public bool ThrowOnNotify { get; init; }

        /// <inheritdoc/>
        public Task NotifyAsync(ContactEnquiry enquiry, CancellationToken ct)
        {
            if (ThrowOnNotify)
            {
                throw new InvalidOperationException("The fake chat channel was told to fail this post.");
            }

            Notified.Add(enquiry);
            return Task.CompletedTask;
        }
    }
}
