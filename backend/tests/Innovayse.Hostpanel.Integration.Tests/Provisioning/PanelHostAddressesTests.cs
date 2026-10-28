using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using Innovayse.Hostpanel.Infrastructure.Provisioning.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// <see cref="PanelNetworkOptions"/> and <see cref="PanelHostAddressConnector"/>: a control panel
/// reached over a private route (a VPN address) while requests, and so TLS, keep its public name.
/// </summary>
public sealed class PanelHostAddressesTests
{
    /// <summary>Pairs parse into a case-insensitive map, whitespace trimmed.</summary>
    [Fact]
    public void HostAddresses_ParseIntoMap()
    {
        var options = new PanelNetworkOptions { HostAddresses = " srv4.example.test = 10.77.0.1 , other.example.test=fd00::1" };

        options.IsValid.Should().BeTrue();
        options.Map.Should().HaveCount(2);
        options.Map["SRV4.example.test"].Should().Be(IPAddress.Parse("10.77.0.1"));
        options.Map["other.example.test"].Should().Be(IPAddress.Parse("fd00::1"));
    }

    /// <summary>Unset means no overrides, and is valid.</summary>
    [Fact]
    public void HostAddresses_Empty_IsValidAndEmpty()
    {
        var options = new PanelNetworkOptions();

        options.IsValid.Should().BeTrue();
        options.Map.Should().BeEmpty();
    }

    /// <summary>A pair without '=', or with something other than an IP address, is rejected.</summary>
    /// <param name="value">The malformed configuration value.</param>
    [Theory]
    [InlineData("srv4.example.test")]
    [InlineData("srv4.example.test=not-an-ip")]
    [InlineData("=10.77.0.1")]
    [InlineData("srv4.example.test=10.77.0.1,srv4.example.test=10.77.0.2")]
    public void HostAddresses_Malformed_IsInvalid(string value)
    {
        new PanelNetworkOptions { HostAddresses = value }.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// A mapped host is dialled at its mapped address: the request goes to a name that does not
    /// resolve and still reaches the local listener, with the original Host header.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MappedHost_IsDialledAtMappedAddress()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var served = ServeOnceAsync(listener);

        var options = Options.Create(new PanelNetworkOptions { HostAddresses = "panel.invalid=127.0.0.1" });
        using var handler = PanelHostAddressConnector.CreateHandler(options);
        using var client = new HttpClient(handler);

        var body = await client.GetStringAsync($"http://panel.invalid:{port}/v1/packages");

        body.Should().Be("ok");
        (await served).Should().Contain($"Host: panel.invalid:{port}");
    }

    /// <summary>Answers one HTTP request with "ok" and returns the request head.</summary>
    /// <param name="listener">The started listener.</param>
    /// <returns>The raw request head.</returns>
    private static async Task<string> ServeOnceAsync(TcpListener listener)
    {
        using var socket = await listener.AcceptSocketAsync();
        var buffer = new byte[4096];
        var read = await socket.ReceiveAsync(buffer);
        var head = Encoding.ASCII.GetString(buffer, 0, read);
        await socket.SendAsync(Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok"));
        return head;
    }
}
