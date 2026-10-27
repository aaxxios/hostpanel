using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;
using Innovayse.Hostpanel.Infrastructure.Integrations.NameAm.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.NameAm;

/// <summary>
/// <see cref="NameAmRegistrarProvider.RenewAsync"/> against a stub Name.am API: an unconfigured
/// integration and an error body are both refusals, never a success that renewed nothing, and a
/// successful renewal leaves the new expiry to the caller (the purchase response carries none).
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host; only its data context is used, which
/// <see cref="NameAmClient"/> reads integration overrides from (none are saved here).</param>
public sealed class NameAmRegistrarProviderTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    /// <summary>The loopback server standing in for the Name.am API.</summary>
    private StubHttpServer _nameAm = null!;

    /// <summary>The scope the provider's data context comes from; disposed with the test.</summary>
    private IServiceScope _scope = null!;

    /// <summary>Starts the stub Name.am server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        _nameAm = await StubHttpServer.StartAsync();
        _scope = factory.Services.CreateScope();
    }

    /// <summary>Stops the stub Name.am server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        _scope.Dispose();
        await _nameAm.DisposeAsync();
    }

    /// <summary>Builds a provider aimed at the stub server.</summary>
    /// <param name="configured">Whether credentials are set.</param>
    /// <returns>The provider under test.</returns>
    private NameAmRegistrarProvider BuildProvider(bool configured = true)
    {
        var options = new NameAmOptions
        {
            Email = configured ? "reseller@example.test" : string.Empty,
            Password = configured ? "secret" : string.Empty,
            ApiUrl = _nameAm.BaseAddress.ToString().TrimEnd('/'),
        };
        var client = new NameAmClient(
            new HttpClient(),
            Options.Create(options),
            _scope.ServiceProvider.GetRequiredService<AppDbContext>(),
            NullLogger<NameAmClient>.Instance);
        return new NameAmRegistrarProvider(client, NullLogger<NameAmRegistrarProvider>.Instance);
    }

    /// <summary>Queues the login reply, then the purchase reply.</summary>
    /// <param name="purchaseJson">The body Name.am answers the purchase with.</param>
    private void ScriptRenewal(string purchaseJson)
    {
        _nameAm.Enqueue("""{"accessToken":"Bearer test-token"}""");
        _nameAm.Enqueue(purchaseJson);
    }

    /// <summary>An unconfigured integration refuses the renewal and calls nothing.</summary>
    [Fact]
    public async Task RenewAsync_WhenNotConfigured_FailsWithoutCallingNameAmAsync()
    {
        var provider = BuildProvider(configured: false);

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.am", "example.am", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
        _nameAm.Requests.Should().BeEmpty();
    }

    /// <summary>An HTTP 200 whose body reports an error is a refusal carrying Name.am's message.</summary>
    [Fact]
    public async Task RenewAsync_WhenBodyReportsAnError_FailsWithTheMessageAsync()
    {
        ScriptRenewal("""{"message":"Insufficient balance"}""");
        var provider = BuildProvider();

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.am", "example.am", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Insufficient balance");
    }

    /// <summary>
    /// A successful purchase reports no expiry, so the caller extends the current one; reporting
    /// now + years would cost an early renewal its remaining days.
    /// </summary>
    [Fact]
    public async Task RenewAsync_WhenPurchased_SucceedsWithoutInventingAnExpiryAsync()
    {
        ScriptRenewal("""[{"domain":"example.am","type":"domain_renew"}]""");
        var provider = BuildProvider();

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.am", "example.am", 2), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ExpiresAt.Should().BeNull();
        var purchase = _nameAm.Requests.Should().Contain(r => r.PathAndQuery.StartsWith("/client/carts/purchase")).Which;
        purchase.Body.Should().Contain("domain_renew").And.Contain("2_year_register");
    }

    /// <summary>An HTTP 200 that is neither a purchase list nor a known error is not proof of a renewal.</summary>
    [Fact]
    public async Task RenewAsync_WhenBodyHasAnUnknownShape_FailsAsync()
    {
        ScriptRenewal("""{"success":false,"errors":["plan unavailable"]}""");
        var provider = BuildProvider();

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.am", "example.am", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("unexpected response");
    }

    /// <summary>A 4xx is Name.am turning the order down: a clean refusal, not an exception.</summary>
    [Fact]
    public async Task RenewAsync_WhenNameAmAnswers4xx_FailsCleanlyAsync()
    {
        _nameAm.Enqueue("""{"accessToken":"Bearer test-token"}""");
        _nameAm.Enqueue(HttpStatusCode.UnprocessableEntity, """{"message":"Insufficient balance"}""");
        var provider = BuildProvider();

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.am", "example.am", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("422");
    }

    /// <summary>A 5xx leaves the outcome unknown, so it stays an exception for the caller to treat as such.</summary>
    [Fact]
    public async Task RenewAsync_WhenNameAmAnswers5xx_ThrowsAsync()
    {
        _nameAm.Enqueue("""{"accessToken":"Bearer test-token"}""");
        _nameAm.Enqueue(HttpStatusCode.BadGateway, "{}");
        var provider = BuildProvider();

        var act = () => provider.RenewAsync(new DomainRenewalOrder("example.am", "example.am", 1), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>A registration on an unconfigured integration is refused, so a paid order is refunded.</summary>
    [Fact]
    public async Task RegisterAsync_WhenNotConfigured_FailsWithoutCallingNameAmAsync()
    {
        var provider = BuildProvider(configured: false);

        var result = await provider.RegisterAsync(
            new DomainRegistrationOrder("example.am", 1, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
        _nameAm.Requests.Should().BeEmpty();
    }

    /// <summary>A registration Name.am turns down with a 4xx is a clean refusal.</summary>
    [Fact]
    public async Task RegisterAsync_WhenNameAmAnswers4xx_FailsCleanlyAsync()
    {
        _nameAm.Enqueue("""{"accessToken":"Bearer test-token"}""");
        _nameAm.Enqueue(HttpStatusCode.BadRequest, """{"message":"Domain is taken"}""");
        var provider = BuildProvider();

        var result = await provider.RegisterAsync(
            new DomainRegistrationOrder("example.am", 1, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("400");
    }

    /// <summary>A purchased registration succeeds and is left for polling to activate.</summary>
    [Fact]
    public async Task RegisterAsync_WhenPurchased_SucceedsPendingPollingAsync()
    {
        ScriptRenewal("""[{"domain":"example.am","type":"domain_registration"}]""");
        var provider = BuildProvider();

        var result = await provider.RegisterAsync(
            new DomainRegistrationOrder("example.am", 1, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.RegistrarRef.Should().Be("example.am");
        result.RequiresPolling.Should().BeTrue();
    }

    /// <summary>A transfer on an unconfigured integration is refused.</summary>
    [Fact]
    public async Task TransferAsync_WhenNotConfigured_FailsAsync()
    {
        var provider = BuildProvider(configured: false);

        var result = await provider.TransferAsync(
            new DomainTransferOrder("example.am", "EPP-1", false), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
    }

    /// <summary>
    /// A change to a registrar-held domain on an unconfigured integration is refused, so the
    /// handler does not save a change the registrar never made.
    /// </summary>
    [Fact]
    public async Task SetNameserversAsync_WhenNotConfigured_FailsAsync()
    {
        var provider = BuildProvider(configured: false);

        var result = await provider.SetNameserversAsync(
            "example.am", "example.am", ["ns1.example.test"], CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
        _nameAm.Requests.Should().BeEmpty();
    }
}
