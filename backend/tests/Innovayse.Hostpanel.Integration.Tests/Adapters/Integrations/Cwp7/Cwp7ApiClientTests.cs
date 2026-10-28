using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Providers.CWP7;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.Cwp7;
/// <summary>
/// Unit tests for <see cref="Cwp7ApiClient"/>'s response parsing.
/// </summary>
/// <remarks>
/// CWP7 answers errors with HTTP 200 and an HTML body, so <c>EnsureSuccessStatusCode</c> passes.
/// The client used to catch the resulting <c>JsonException</c> and return
/// <c>Status = "OK"</c>, turning a CWP7 error page into a successfully provisioned account —
/// and it logged that at Debug, so at production log levels it recorded nothing at all.
/// </remarks>
public sealed class Cwp7ApiClientTests : IAsyncLifetime
{
    /// <summary>The loopback server standing in for the CWP7 host; started per test class instance.</summary>
    private StubHttpServer cwp7 = null!;

    /// <summary>Gets the stub server address without the trailing slash, as the client expects a base URL.</summary>
    private string BaseUrl => cwp7.BaseAddress.ToString().TrimEnd('/');

    /// <summary>Starts the stub CWP7 server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        cwp7 = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub CWP7 server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await cwp7.DisposeAsync();
    }

    /// <summary>A CWP7 error page: HTTP 200, HTML body, no JSON anywhere.</summary>
    private const string ErrorPageHtml =
        """
        <!DOCTYPE html>
        <html><head><title>500 Internal Server Error</title></head>
        <body><h1>Internal Server Error</h1>
        <p>The API key provided is not authorized for this module.</p>
        </body></html>
        """;

    /// <summary>
    /// Builds a client aimed at the stub server, which answers the given body with HTTP 200 over real HTTP.
    /// </summary>
    /// <param name="body">The response body CWP7 answers with.</param>
    /// <param name="mediaType">The Content-Type to answer with.</param>
    /// <returns>The configured client.</returns>
    private Cwp7ApiClient BuildClient(string body, string mediaType)
    {
        cwp7.Enqueue(HttpStatusCode.OK, body, mediaType);
        return new Cwp7ApiClient(new HttpClient(), NullLogger<Cwp7ApiClient>.Instance);
    }

    /// <summary>Verifies: Create Account - Fails - When Cwp7Answers With An Html Error Page.</summary>
    /// <summary>Create account, fails, when cwp7 answers with an html error page.</summary>
    [Fact]
    public async Task CreateAccount_Fails_WhenCwp7AnswersWithAnHtmlErrorPageAsync()
    {
        var client = BuildClient(ErrorPageHtml, "text/html");

        var result = await client.CreateAccountAsync(
            BaseUrl, "test-key", "example.com", "testuser", "pass123",
            "default", "test@example.com", "0", "40", "150", "203.0.113.10",
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("non-JSON");
    }

    /// <summary>Suspend Account, fails, when Cwp7 Answers With An Html Error Page.</summary>
    /// <summary>Verifies: Suspend Account - Fails - When Cwp7Answers With An Html Error Page.</summary>
    [Fact]
    public async Task SuspendAccount_Fails_WhenCwp7AnswersWithAnHtmlErrorPageAsync()
    {
        var client = BuildClient(ErrorPageHtml, "text/html");

        var result = await client.SuspendAccountAsync(
            BaseUrl, "test-key", "testuser", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>Verifies: Create Account - Succeeds - When Cwp7Answers With The Json Envelope.</summary>
    [Fact]
    public async Task CreateAccount_Succeeds_WhenCwp7AnswersWithTheJsonEnvelopeAsync()
    {
        var client = BuildClient("""{ "status": "OK", "msj": "Account created." }""", "application/json");

        var result = await client.CreateAccountAsync(
            BaseUrl, "test-key", "example.com", "testuser", "pass123",
            "default", "test@example.com", "0", "40", "150", "203.0.113.10",
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Account created.");
    }

    /// <summary>
    /// A real <c>/v1/user_session</c> body, captured from a live CWP7 server. Its <c>msj</c> is an
    /// object, unlike every other endpoint's, which is the whole reason this call needs its own
    /// parsing.
    /// </summary>
    private const string UserSessionJson =
        """
        {"status":"OK","msj":{"accounts":1,"details":[{"user":"rootsage",
        "token":"eac44313.f2acd74d.3d04bdcb.1788432569",
        "url":"https://host3.example.com:2083/rootsage/?user_session=eac44313.f2acd74d.3d04bdcb.1788432569"}]}}
        """;

    /// <summary>Verifies: Auto Login - Returns The Session Url - When Cwp7Issues One.</summary>
    [Fact]
    public async Task AutoLogin_ReturnsTheSessionUrl_WhenCwp7IssuesOneAsync()
    {
        var client = BuildClient(UserSessionJson, "application/json");

        var result = await client.GetAutoLoginUrlAsync(
            BaseUrl, "test-key", "rootsage", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be(
            "https://host3.example.com:2083/rootsage/?user_session=eac44313.f2acd74d.3d04bdcb.1788432569");
    }

    /// <summary>Verifies: Auto Login - Fails - When The Session Is For ADifferent User.</summary>
    [Fact]
    public async Task AutoLogin_Fails_WhenTheSessionIsForADifferentUserAsync()
    {
        // A session on somebody else's panel is worse than no session: the button would have
        // opened it and signed this client in as them.
        var client = BuildClient(UserSessionJson, "application/json");

        var result = await client.GetAutoLoginUrlAsync(
            BaseUrl, "test-key", "someoneelse", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>Verifies: Auto Login - Fails - When Cwp7Refuses With AString Message.</summary>
    [Fact]
    public async Task AutoLogin_Fails_WhenCwp7RefusesWithAStringMessageAsync()
    {
        // The refusal shape: `msj` is a string here, where a success carries an object. Reading
        // one into the other throws, and that must read as "no session", not as a crash.
        var client = BuildClient(
            """{"status":"Error","msj":"Unauthorized action abc123","format":"JSON"}""",
            "application/json");

        var result = await client.GetAutoLoginUrlAsync(
            BaseUrl, "test-key", "rootsage", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Unauthorized action");
    }

    /// <summary>Verifies: Auto Login - Fails - When The Endpoint Is Not Served.</summary>
    [Fact]
    public async Task AutoLogin_Fails_WhenTheEndpointIsNotServedAsync()
    {
        // What every client actually got until this was fixed: the call named a route CWP7 does
        // not serve, and its 404 page came back with HTTP 200.
        var client = BuildClient(
            "<html><head><title>404 Page Not Found</title></head><body></body></html>",
            "text/html");

        /// <summary>Auto Login, calls The User Session Route.</summary>
        var result = await client.GetAutoLoginUrlAsync(
            BaseUrl, "test-key", "rootsage", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>Verifies: Auto Login - Calls The User Session Route.</summary>
    [Fact]
    public async Task AutoLogin_CallsTheUserSessionRouteAsync()
    {
        // The bug was the address, not the parsing, so the address is what this pins down.
        var client = BuildClient(UserSessionJson, "application/json");

        await client.GetAutoLoginUrlAsync(
            BaseUrl, "test-key", "rootsage", CancellationToken.None);

        cwp7.Requests.Should().ContainSingle().Which.PathAndQuery.Should().Be("/v1/user_session");
    }

    /// <summary>
    /// A packages-list refusal ("Error" with a text <c>msj</c>) surfaces CWP7's own reason instead
    /// of a JSON conversion error, so a wrong API key reads as one in the server log.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetServerInfo_Refusal_ThrowsWithCwpReason()
    {
        var client = new Cwp7ApiClient(
            new HttpClient(new FixedReplyHandler("""{"status":"Error","msj":"key does not exist"}""")),
            NullLogger<Cwp7ApiClient>.Instance);

        var act = () => client.GetServerInfoAsync("panel.example.test", "2304", "wrong-key", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*key does not exist*");
    }

    /// <summary>A packages list answers with its entry count.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetServerInfo_PackageList_ReturnsCount()
    {
        var client = new Cwp7ApiClient(
            new HttpClient(new FixedReplyHandler("""{"status":"OK","msj":[{"id":1},{"id":2}]}""")),
            NullLogger<Cwp7ApiClient>.Instance);

        var count = await client.GetServerInfoAsync("panel.example.test", "2304", "key", CancellationToken.None);

        count.Should().Be(2);
    }

    /// <summary>Answers every request with the same JSON body, without a network round trip.</summary>
    /// <param name="body">The JSON body to answer with.</param>
    private sealed class FixedReplyHandler(string body) : HttpMessageHandler
    {
        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
            });
    }
}
