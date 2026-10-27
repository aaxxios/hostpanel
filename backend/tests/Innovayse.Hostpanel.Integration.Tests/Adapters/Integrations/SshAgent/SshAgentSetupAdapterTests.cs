using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;
using Microsoft.Extensions.Logging.Abstractions;
using Renci.SshNet;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.SshAgent;

/// <summary>The setup adapters: key generation, the bundle and the host key scanner.</summary>
public sealed class SshAgentSetupAdapterTests
{
    [Fact]
    public void Generator_ProducesDistinctLoadableEd25519Pairs()
    {
        var generator = new BouncyCastleSshIdentityGenerator();

        var a = generator.Generate("hostpanel-server-1");
        var b = generator.Generate("hostpanel-server-1; rm -rf /");

        a.PublicKey.Should().NotBe(b.PublicKey);
        b.PublicKey.Should().EndWith("hostpanel-server-1rm-rf");
        a.PrivateKey.Should().StartWith("-----BEGIN OPENSSH PRIVATE KEY-----\n");
        using var keyFile = new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(a.PrivateKey)));
        keyFile.Key.ToString().Should().Be("ssh-ed25519");
        a.PublicKey.Split(' ')[1].Should().Be(Convert.ToBase64String(((Renci.SshNet.Security.KeyHostAlgorithm)keyFile.HostKeyAlgorithms.First()).Data));
    }

    [Fact]
    public void Bundle_IsDeterministicAndVersioned()
    {
        var first = new EmbeddedSshAgentBundle();
        var second = new EmbeddedSshAgentBundle();

        first.Sha256.Should().Be(second.Sha256).And.MatchRegex("^[0-9a-f]{64}$");
        first.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+$");
        first.Profiles.Should().BeEquivalentTo(["cwp", "generic-linux"]);
    }

    [Fact]
    public async Task Scanner_ReportsNullForAClosedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var scan = await new SshNetHostKeyScanner(NullLogger<SshNetHostKeyScanner>.Instance)
            .ScanAsync("127.0.0.1", port, TimeSpan.FromSeconds(2), CancellationToken.None);

        scan.Should().BeNull();
    }
}
