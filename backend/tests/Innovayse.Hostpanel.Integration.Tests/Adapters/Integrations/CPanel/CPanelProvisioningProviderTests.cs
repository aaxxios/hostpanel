using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Domain.Provisioning.Enums;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.CPanel;

/// <summary>
/// Tests for <see cref="CPanelProvisioningProvider"/> against a stub WHM server: a refusal that
/// really means "the account is already like that" is a success, and every other refusal still fails.
/// </summary>
public sealed class CPanelProvisioningProviderTests : IAsyncLifetime
{
    /// <summary>A WHM v1 success envelope with no payload.</summary>
    private const string Accepted = """{ "metadata": { "result": 1, "reason": "Ok", "version": 1 } }""";

    /// <summary>A WHM v1 refusal.</summary>
    private const string Refused = """{ "metadata": { "result": 0, "reason": "Refused by test", "version": 1 } }""";

    /// <summary>An account summary for an active account.</summary>
    private const string ActiveSummary =
        """
        { "metadata": { "result": 1, "version": 1 },
          "data": { "acct": [ { "user": "testuser", "domain": "example.com", "suspended": 0 } ] } }
        """;

    /// <summary>An account summary for a suspended account.</summary>
    private const string SuspendedSummary =
        """
        { "metadata": { "result": 1, "version": 1 },
          "data": { "acct": [ { "user": "testuser", "domain": "example.com", "suspended": 1 } ] } }
        """;

    /// <summary>An account summary that lists nothing.</summary>
    private const string EmptySummary = """{ "metadata": { "result": 1, "version": 1 }, "data": { "acct": [] } }""";

    /// <summary>The order every provision test sends.</summary>
    private static readonly ProvisionAccountOrder Order = new(7, "example.com", "testuser", "pass123", "default");

    /// <summary>The loopback server standing in for the WHM host.</summary>
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

    /// <summary>Builds a provider whose WHM answers the given bodies in order.</summary>
    /// <param name="answers">The JSON bodies, one per expected call.</param>
    /// <returns>The provider under test.</returns>
    private CPanelProvisioningProvider BuildProvider(params string[] answers)
    {
        foreach (var answer in answers)
        {
            whm.Enqueue(answer);
        }

        var http = new HttpClient { BaseAddress = whm.BaseAddress };
        var client = new CPanelClient(http, NullLogger<CPanelClient>.Instance);
        var options = Options.Create(new CPanelOptions
        {
            ApiUrl = "https://whm.test:2087",
            Username = "root",
            ApiToken = "test-token",
            ServerIp = "203.0.113.10",
        });
        return new CPanelProvisioningProvider(client, options);
    }

    /// <summary>A created account is reported created, with the username as reference.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Provision_ReturnsCreated_WhenWhmAcceptsAsync()
    {
        var provider = BuildProvider(Accepted);

        var result = await provider.ProvisionAsync(Order, CancellationToken.None);

        result.Should().Be(ProvisioningResult.Created("testuser"));
        whm.Requests.Should().ContainSingle();
    }

    /// <summary>A retry after a lost answer meets the account it already made.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Provision_ReturnsAlreadyExistsWithTheDomain_WhenCreateIsRefusedAndTheAccountExistsAsync()
    {
        var provider = BuildProvider(Refused, ActiveSummary);

        var result = await provider.ProvisionAsync(Order, CancellationToken.None);

        result.Outcome.Should().Be(ProvisioningOutcome.AlreadyExists);
        result.ProvisioningRef.Should().Be("testuser");
        result.ExistingAccountDomain.Should().Be("example.com");
    }

    /// <summary>A refusal with no account behind it stays a failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Provision_Throws_WhenCreateIsRefusedAndNoAccountExistsAsync()
    {
        var provider = BuildProvider(Refused, EmptySummary);

        var act = () => provider.ProvisionAsync(Order, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>A refused lookup leaves the original refusal as the failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Provision_Throws_WhenCreateIsRefusedAndTheLookupIsRefusedToAsync()
    {
        var provider = BuildProvider(Refused, Refused);

        var act = () => provider.ProvisionAsync(Order, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>Suspending an account that is already suspended is a success.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Suspend_Succeeds_WhenWhmRefusesBecauseTheAccountIsAlreadySuspendedAsync()
    {
        var provider = BuildProvider(Refused, SuspendedSummary);

        var act = () => provider.SuspendAsync(new SuspendAccountOrder(7, "testuser", "overdue"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>A refused suspend on an active account is a real failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Suspend_Throws_WhenWhmRefusesAndTheAccountIsActiveAsync()
    {
        var provider = BuildProvider(Refused, ActiveSummary);

        var act = () => provider.SuspendAsync(new SuspendAccountOrder(7, "testuser", "overdue"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>Unsuspending an account that is already active is a success.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Unsuspend_Succeeds_WhenWhmRefusesBecauseTheAccountIsAlreadyActiveAsync()
    {
        var provider = BuildProvider(Refused, ActiveSummary);

        var act = () => provider.UnsuspendAsync("testuser", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>A refused unsuspend on a suspended account is a real failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Unsuspend_Throws_WhenWhmRefusesAndTheAccountIsStillSuspendedAsync()
    {
        var provider = BuildProvider(Refused, SuspendedSummary);

        var act = () => provider.UnsuspendAsync("testuser", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>Terminating an account that is already gone is a success.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Terminate_Succeeds_WhenWhmRefusesBecauseTheAccountIsAlreadyGoneAsync()
    {
        var provider = BuildProvider(Refused, EmptySummary);

        var act = () => provider.TerminateAsync(new TerminateAccountOrder(7, "testuser", "cancelled"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>A refused removal of an account that still exists is a real failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Terminate_Throws_WhenWhmRefusesAndTheAccountStillExistsAsync()
    {
        var provider = BuildProvider(Refused, ActiveSummary);

        var act = () => provider.TerminateAsync(new TerminateAccountOrder(7, "testuser", "cancelled"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>A failed lookup is never read as "already gone".</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Terminate_Throws_WhenWhmRefusesAndTheLookupIsRefusedToAsync()
    {
        var provider = BuildProvider(Refused, Refused);

        var act = () => provider.TerminateAsync(new TerminateAccountOrder(7, "testuser", "cancelled"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>The credentials carry no password and the domain WHM reports for the account.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task GetCredentials_ReturnsTheDomainWhmReportsAndNoPasswordAsync()
    {
        var provider = BuildProvider(ActiveSummary);

        var credentials = await provider.GetCredentialsAsync("testuser", CancellationToken.None);

        credentials.Password.Should().BeNull();
        credentials.Username.Should().Be("testuser");
        credentials.Domain.Should().Be("example.com");
        credentials.ServerIp.Should().Be("203.0.113.10");
        credentials.CpanelUrl.Should().Be("https://whm.test:2087/cpanel");
    }

    /// <summary>An account WHM does not list is a failed read, not an invented domain.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task GetCredentials_Throws_WhenWhmListsNoSuchAccountAsync()
    {
        var provider = BuildProvider(EmptySummary);

        var act = () => provider.GetCredentialsAsync("testuser", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
