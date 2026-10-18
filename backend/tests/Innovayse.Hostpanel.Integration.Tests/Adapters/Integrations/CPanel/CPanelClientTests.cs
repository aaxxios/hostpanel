using FluentAssertions;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel.Settings;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.CPanel;
/// <summary>
/// Unit tests for <see cref="CPanelClient"/>'s WHM JSON API v1 envelope handling.
/// </summary>
/// <remarks>
/// The client asks for <c>api.version=1</c>, whose refusals arrive as HTTP 200 with
/// <c>{"metadata":{"result":0,"reason":"…"}}</c>. It used to look for the API v0
/// <c>result.status</c> / <c>statusmsg</c> pair, which a v1 body never carries, so every WHM
/// refusal returned as a success and all seven functions were unable to report failure.
/// </remarks>
public sealed class CPanelClientTests : IAsyncLifetime
{
    /// <summary>Settings pointing the client at a WHM host that does not exist.</summary>
    private static CPanelOptions Settings => new()
    {
        ApiUrl = "https://whm.test:2087",
        Username = "root",
        ApiToken = "test-token",
        ServerIp = "203.0.113.10",
    };

    /// <summary>The loopback server standing in for the WHM host; started per test class instance.</summary>
    private StubHttpServer whm = null!;

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
        return new CPanelClient(http, Options.Create(Settings));
    }

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

    /// <summary>Create account, throws, when whm metadata result is zero.</summary>
    [Fact]
    public async Task CreateAccount_Throws_WhenWhmMetadataResultIsZeroAsync()
    {
        var client = BuildClient(CreateAcctRefused);

        var act = async () => await client.CreateAccountAsync(
            "example.com", "testuser", "pass123", "default", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*createacct*")
            .WithMessage("*already exists*");
    }

    /// <summary>Create account, does not throw, when whm metadata result is one.</summary>
    [Fact]
    public async Task CreateAccount_DoesNotThrow_WhenWhmMetadataResultIsOneAsync()
    {
        var client = BuildClient(CreateAcctAccepted);

        var act = async () => await client.CreateAccountAsync(
            "example.com", "testuser", "pass123", "default", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>Suspend account, throws, when whm refuses.</summary>
    [Fact]
    public async Task SuspendAccount_Throws_WhenWhmRefusesAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 0, "reason": "Access denied", "version": 1 } }""");

        var act = async () => await client.SuspendAccountAsync("testuser", "non-payment", CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*Access denied*");
    }

    /// <summary>Call api, throws, when response carries no metadata envelope.</summary>
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
    [Fact]
    public async Task RemoveAccount_SendsTheUserParameterWhmActuallyReadsAsync()
    {
        var client = BuildClient(
            """{ "metadata": { "result": 1, "reason": "Ok", "version": 1 } }""");

        await client.RemoveAccountAsync("testuser", CancellationToken.None);

        // WHM's removeacct names the account "user". "username" is ignored and the account survives.
        var query = whm.Requests.Should().ContainSingle().Which.PathAndQuery;
        query.Should().Contain("user=testuser");
        query.Should().NotContain("username=");
    }
}
