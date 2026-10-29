using System.Net;
using Innovayse.Hostpanel.Infrastructure.Security;
using Innovayse.Hostpanel.Infrastructure.Security.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Security;

/// <summary>
/// <see cref="OutboundConnector"/>: a refused address is never dialled, private networks follow the
/// option, and an HTTP client behind the connector cannot reach a loopback server at all.
/// </summary>
public sealed class OutboundConnectorTests
{
    /// <summary>Loopback and metadata hosts are refused before any connection is opened.</summary>
    /// <param name="host">The host under test.</param>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.169.254")]
    [InlineData("localhost")]
    public async Task ResolveAsync_RefusedHost_ThrowsAsync(string host)
    {
        var act = () => Connector(allowPrivate: true).ResolveAsync(host, CancellationToken.None);

        await act.Should().ThrowAsync<OutboundAddressRefusedException>();
    }

    /// <summary>A private address is allowed by default and refused once private networks are switched off.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ResolveAsync_PrivateAddress_FollowsTheOptionAsync()
    {
        var allowed = await Connector(allowPrivate: true).ResolveAsync("10.0.0.5", CancellationToken.None);
        var act = () => Connector(allowPrivate: false).ResolveAsync("10.0.0.5", CancellationToken.None);

        allowed.Should().ContainSingle().Which.Should().Be(IPAddress.Parse("10.0.0.5"));
        await act.Should().ThrowAsync<OutboundAddressRefusedException>();
    }

    /// <summary>The private networks option is on unless a deployment switches it off.</summary>
    [Fact]
    public void Options_AllowPrivateNetworksByDefault()
    {
        new OutboundNetworkOptions().AllowPrivateNetworks.Should().BeTrue();
    }

    /// <summary>An HTTP client behind the connector never reaches a server on loopback.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ConnectAsync_LoopbackServer_IsNeverReachedAsync()
    {
        await using var stub = await StubHttpServer.StartAsync();
        stub.Enqueue("{}");
        using var client = new HttpClient(new SocketsHttpHandler { ConnectCallback = Connector(allowPrivate: true).ConnectAsync })
        {
            BaseAddress = stub.BaseAddress,
        };

        var act = () => client.GetAsync("probe");

        await act.Should().ThrowAsync<HttpRequestException>();
        stub.Requests.Should().BeEmpty();
    }

    /// <summary>Builds a connector with the given private network setting.</summary>
    /// <param name="allowPrivate">Whether private networks are allowed.</param>
    /// <returns>The connector.</returns>
    private static OutboundConnector Connector(bool allowPrivate) =>
        new(Options.Create(new OutboundNetworkOptions { AllowPrivateNetworks = allowPrivate }), Options.Create(new PanelNetworkOptions()));
}
