using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.CPanel;

/// <summary>
/// Unit tests for <see cref="CPanelClient"/>'s WHM JSON API v1 envelope handling.
/// </summary>
/// <remarks>
/// The client asks for <c>api.version=1</c>, whose refusals arrive as HTTP 200 with
/// <c>{"metadata":{"result":0,"reason":"text"}}</c>. These tests pin that every one of the functions
/// reports such a refusal as a failure, that secrets travel in the POST body and never in the
/// URL, and that the panel's own reason reaches the log and not the exception.
/// </remarks>
public sealed class CPanelClientTests : IAsyncLifetime
{
    /// <summary>A real WHM v1 refusal: HTTP 200, result 0, reason in the metadata.</summary>
    private const string CreateAcctRefused =
        """
        {
          "metadata": {
            "command": "createacct",
            "reason": "The account 'testuser' already exists.",
            "result": 0,
            "version": 1
          },
          "data": {}
        }
        """;

    /// <summary>A real WHM v1 success envelope.</summary>
    private const string CreateAcctAccepted =
        """
        {
          "metadata": {
            "command": "createacct",
            "reason": "Account Creation Ok",
            "result": 1,
            "version": 1
          },
          "data": { "ip": "203.0.113.10", "nameserver": "ns1.test" }
        }
        """;

    /// <summary>The loopback server standing in for the WHM host; started per test class instance.</summary>
    private StubHttpServer whm = null!;

    /// <summary>Captures what the client logs, so a test can assert on the refusal reason.</summary>
    private readonly CapturingLogger<CPanelClient> logger = new();

    /// <summary>Starts the stub WHM server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        whm = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub WHM server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await whm.DisposeAsync();
    }

    /// <summary>
    /// Builds a client aimed at the stub server, which answers <paramref name="responseJson"/>
    /// over real HTTP. The request the client sent is read back from <see cref="whm"/>.
    /// </summary>
    /// <param name="responseJson">The JSON body WHM answers with.</param>
    /// <returns>The client under test.</returns>
    private CPanelClient BuildClient(string responseJson)
    {
        whm.Enqueue(responseJson);
        var http = new HttpClient { BaseAddress = whm.BaseAddress };
        return new CPanelClient(http, logger);
    }

    /// <summary>Create account, throws, when whm metadata result is zero.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CreateAccount_Throws_WhenWhmMetadataResultIsZeroAsync()
    {
        var client = BuildClient(CreateAcctRefused);

        var act = async () => await client.CreateAccountAsync(
            "example.com", "testuser", "pass123", "default", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*createacct*");
    }

    /// <summary>The refusal reason is logged and kept out of the exception callers pass on.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CreateAccount_LogsTheReasonButKeepsItOutOfTheException_WhenWhmRefusesAsync()
    {
        var client = BuildClient(CreateAcctRefused);

        var act = async () => await client.CreateAccountAsync(
            "example.com", "testuser", "pass123", "default", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().NotContain("already exists").And.NotContain("testuser");
        logger.Messages.Should().ContainSingle().Which.Should().Contain("already exists");
    }

    /// <summary>Create account, does not throw, when whm metadata result is one.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CreateAccount_DoesNotThrow_WhenWhmMetadataResultIsOneAsync()
    {
        var client = BuildClient(CreateAcctAccepted);

        var act = async () => await client.CreateAccountAsync(
            "example.com", "testuser", "pass123", "default", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>The password travels in a POST body, and the URL carries only the function name.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CreateAccount_SendsThePasswordInAPostBody_NeverInTheUrlAsync()
    {
        var client = BuildClient(CreateAcctAccepted);

        await client.CreateAccountAsync("example.com", "testuser", "pass123", "default", CancellationToken.None);

        var call = whm.Requests.Should().ContainSingle().Which;
        call.Method.Should().Be("POST");
        call.PathAndQuery.Should().Be("/json-api/createacct");
        call.Body.Should().Contain("password=pass123").And.Contain("api.version=1");
    }

    /// <summary>A password change keeps the new password out of the URL as well.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ChangePassword_SendsTheNewPasswordInAPostBody_NeverInTheUrlAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 1, "reason": "Ok", "version": 1 } }""");

        await client.ChangePasswordAsync("testuser", "n3w-Secret", CancellationToken.None);

        var call = whm.Requests.Should().ContainSingle().Which;
        call.PathAndQuery.Should().NotContain("n3w-Secret");
        call.Body.Should().Contain("password=n3w-Secret").And.Contain("user=testuser");
    }

    /// <summary>Suspend account, throws, when whm refuses.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task SuspendAccount_Throws_WhenWhmRefusesAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 0, "reason": "Access denied", "version": 1 } }""");

        var act = async () => await client.SuspendAccountAsync("testuser", "non-payment", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        logger.Messages.Should().ContainSingle().Which.Should().Contain("Access denied");
    }

    /// <summary>Call api, throws, when response carries no metadata envelope.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CallApi_Throws_WhenResponseCarriesNoMetadataEnvelopeAsync()
    {
        // The old API v0 shape. It is not a v1 success, and must not be read as one.
        var client = BuildClient("""{ "result": { "status": 1, "statusmsg": "ok" } }""");

        var act = async () => await client.UnsuspendAccountAsync("testuser", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*metadata.result*");
    }

    /// <summary>Remove account, sends the user parameter whm actually reads.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task RemoveAccount_SendsTheUserParameterWhmActuallyReadsAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 1, "reason": "Ok", "version": 1 } }""");

        await client.RemoveAccountAsync("testuser", CancellationToken.None);

        // WHM's removeacct names the account "user". "username" is ignored and the account survives.
        var body = whm.Requests.Should().ContainSingle().Which.Body;
        body.Should().Contain("user=testuser");
        body.Should().NotContain("username=");
    }

    /// <summary>An account summary carries the domain and the suspension flag.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task FindAccount_ReturnsTheDomainAndSuspensionFlag_WhenWhmListsTheAccountAsync()
    {
        var client = BuildClient(
            """
            { "metadata": { "result": 1, "version": 1 },
              "data": { "acct": [ { "user": "testuser", "domain": "example.com", "suspended": 1 } ] } }
            """);

        var account = await client.FindAccountAsync("testuser", CancellationToken.None);

        account.Should().Be(new WhmAccountSummary("testuser", "example.com", true));
    }

    /// <summary>A successful summary with no accounts means the account does not exist.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task FindAccount_ReturnsNull_WhenWhmListsNoAccountAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 1, "version": 1 }, "data": { "acct": [] } }""");

        var account = await client.FindAccountAsync("testuser", CancellationToken.None);

        account.Should().BeNull();
    }

    /// <summary>A refused summary is a failure, never "no such account".</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task FindAccount_Throws_WhenWhmRefusesAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 0, "reason": "Access denied", "version": 1 } }""");

        var act = async () => await client.FindAccountAsync("testuser", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>A single sign-on answer without a url is a failure, not a missing-property crash.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task GetCPanelSsoUrl_Throws_WhenTheAnswerCarriesNoUrlAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 1, "version": 1 }, "data": {} }""");

        var act = async () => await client.GetCPanelSsoUrlAsync("testuser", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*SSO URL was empty*");
    }

    /// <summary>A single sign-on answer with a url returns it.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task GetCPanelSsoUrl_ReturnsTheUrl_WhenWhmIssuesOneAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 1, "version": 1 }, "data": { "url": "https://whm.test:2083/cpsess1/" } }""");

        var url = await client.GetCPanelSsoUrlAsync("testuser", CancellationToken.None);

        url.Should().Be("https://whm.test:2083/cpsess1/");
    }
}
