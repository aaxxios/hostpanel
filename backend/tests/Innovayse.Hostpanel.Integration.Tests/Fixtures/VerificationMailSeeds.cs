using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Notifications;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Shared plumbing for the email-verification tests: a client on a host whose mail relay is a
/// <see cref="FakeEmailSender"/>, a locally registered account that is signed in, and the pieces
/// of the verification link read back out of the mail the account was sent.
/// </summary>
public static class VerificationMailSeeds
{
    /// <summary>A password that satisfies the local password policy.</summary>
    public const string Password = "Password123!";

    /// <summary>Matches the link behind the verification button, whichever app it points at.</summary>
    private static readonly Regex LinkPattern = new("href=\"([^\"]*(?:confirm-email|verify-email)[^\"]*)\"", RegexOptions.Compiled);

    /// <summary>Builds a client on a host whose mail relay is the given fake.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="sender">The fake relay the host sends through.</param>
    /// <returns>An unauthenticated client of that host.</returns>
    public static HttpClient ClientWithRelay(IntegrationTestFactory factory, FakeEmailSender sender) =>
        EventHandlerTestHost.WithFakeRelay(factory, sender).CreateClient();

    /// <summary>Registers a brand-new local account through the public endpoint.</summary>
    /// <param name="client">The client to register with.</param>
    /// <returns>The new account's email address.</returns>
    public static async Task<string> RegisterAsync(HttpClient client)
    {
        var email = $"verify-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new { email, password = Password, firstName = "Ver", lastName = "Ify" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return email;
    }

    /// <summary>Signs the account in and puts its access token on the client.</summary>
    /// <param name="client">The client that carries the session from now on.</param>
    /// <param name="email">The account's email address.</param>
    /// <returns>A task that completes once the client is authenticated.</returns>
    public static async Task SignInAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.GetProperty("accessToken").GetString());
    }

    /// <summary>Reads whether the signed-in account's address is confirmed.</summary>
    /// <param name="client">An authenticated client.</param>
    /// <returns><see langword="true"/> when the address is confirmed.</returns>
    public static async Task<bool> IsVerifiedAsync(HttpClient client)
    {
        var body = await client.GetFromJsonAsync<JsonElement>("/api/auth/email-verified");
        return body.GetProperty("verified").GetBoolean();
    }

    /// <summary>The verification mails sent to an address, oldest first.</summary>
    /// <param name="sender">The fake relay that recorded the mail.</param>
    /// <param name="email">The recipient.</param>
    /// <returns>Only the mails that carry a verification link.</returns>
    public static IReadOnlyList<EmailMessage> VerificationMailsTo(FakeEmailSender sender, string email) =>
        [.. sender.Sent.Where(m => m.To == email && LinkPattern.IsMatch(m.Body))];

    /// <summary>Takes the verification link out of a mail.</summary>
    /// <param name="message">A mail from <see cref="VerificationMailsTo"/>.</param>
    /// <returns>The link, HTML-decoded, as a browser would follow it.</returns>
    public static Uri LinkOf(EmailMessage message) =>
        new(WebUtility.HtmlDecode(LinkPattern.Match(message.Body).Groups[1].Value));

    /// <summary>Reads one query parameter of a link, URL-decoded.</summary>
    /// <param name="link">The verification link.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The decoded value, or <see langword="null"/> when the link has none.</returns>
    public static string? QueryOf(Uri link, string name) => HttpUtility.ParseQueryString(link.Query)[name];
}
