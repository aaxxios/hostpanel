using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;
using System.Text.Json.Nodes;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.SshAgent;

/// <summary>
/// The agent contract as HostPanel speaks it, against a recording transport. The agent's own
/// side of the contract is tested in <c>ssh-key-agent/tests</c>.
/// </summary>
public sealed class AgentSshKeyProviderTests
{
    private static readonly SshAgentEndpoint Endpoint = new(
        7, "agent.example.net", 2200, "hpsshkeys", "PRIVATE", "SHA256:abc", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

    [Fact]
    public async Task Add_SendsVersionedRequestForTheAccountToItsOwnEndpointAsync()
    {
        var transport = new RecordingTransport("""{"v":1,"ok":true,"changed":true}""");
        var keyId = Guid.NewGuid();

        await new AgentSshKeyProvider(Endpoint, transport).AddKeyAsync(new SshAccount(7, "web"), keyId, "ssh-ed25519 AAAA", default);

        var (endpoint, request) = Assert.Single(transport.Sent);
        Assert.Same(Endpoint, endpoint);
        Assert.Equal(1, request["v"]!.GetValue<int>());
        Assert.Equal("add", request["op"]!.GetValue<string>());
        Assert.Equal("web", request["account"]!.GetValue<string>());
        Assert.Equal(keyId.ToString(), request["key_id"]!.GetValue<string>());
        Assert.Equal(5, request.Count);
    }

    [Fact]
    public async Task AccountOfAnotherServer_RefusedBeforeAnythingIsSentAsync()
    {
        var transport = new RecordingTransport("""{"v":1,"ok":true}""");
        var provider = new AgentSshKeyProvider(Endpoint, transport);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.RevokeKeyAsync(new SshAccount(8, "web"), Guid.NewGuid(), default));
        Assert.Empty(transport.Sent);
    }

    [Fact]
    public async Task List_ParsesManagedKeysAsync()
    {
        var id = Guid.NewGuid();
        var transport = new RecordingTransport($$"""{"v":1,"ok":true,"keys":[{"key_id":"{{id}}","fingerprint":"SHA256:f"},{"key_id":"bogus"}],"unmanaged_keys":2}""");

        var keys = await new AgentSshKeyProvider(Endpoint, transport).ListKeysAsync(new SshAccount(7, "web"), default);

        Assert.Equal(new InstalledSshKey(id, "SHA256:f"), Assert.Single(keys));
    }

    [Theory]
    [InlineData("""{"v":1,"ok":false,"error":"duplicate_key"}""", "duplicate_key", false)]
    [InlineData("""{"v":2,"ok":true}""", "unsupported_version", false)]
    [InlineData("not json", "internal_error", true)]
    [InlineData("""{"v":1,"ok":false,"error":"timeout"}""", "timeout", true)]
    public async Task Refusals_BecomeProviderExceptionsWithCodesAsync(string response, string code, bool unknown)
    {
        var provider = new AgentSshKeyProvider(Endpoint, new RecordingTransport(response));

        var ex = await Assert.ThrowsAsync<SshProviderException>(() => provider.GetAccessStatusAsync(new SshAccount(7, "web"), default));

        Assert.Equal(code, ex.Code);
        Assert.Equal(unknown, ex.IsOutcomeUnknown);
    }

    [Fact]
    public void Factory_OnlyCwpSupported_AndSettingsMustBelongToTheServer()
    {
        var factory = new SshKeyProviderFactory(new RecordingTransport("{}"));
        var cwp = Server.Create(Details(ServerModule.Cwp7));
        var settings = ServerSshSettings.CreateFor(cwp.Id);
        settings.Configure(true, "h", 22, "h", 22, "hp", "KEY", "SHA256:x", 5, 5);

        Assert.IsType<AgentSshKeyProvider>(factory.CreateFor(cwp, settings));
        Assert.Null(factory.CreateFor(Server.Create(Details(ServerModule.CPanel)), settings));
        Assert.Throws<InvalidOperationException>(() => factory.CreateFor(cwp, ServerSshSettings.CreateFor(99)));
    }

    private static ServerDetails Details(ServerModule module) => new(
        "s", "h", null, null, module, "root", null, null, null, true, null, false, false, 0m,
        null, null, null, null, null, null, null, null, null, null, null, null);

    /// <summary>Records requests and answers a fixed response.</summary>
    /// <param name="response">The response to answer.</param>
    private sealed class RecordingTransport(string response) : ISshAgentTransport
    {
        public List<(SshAgentEndpoint Endpoint, JsonObject Request)> Sent { get; } = [];

        public Task<string> SendAsync(SshAgentEndpoint endpoint, string requestJson, CancellationToken ct)
        {
            Sent.Add((endpoint, JsonNode.Parse(requestJson)!.AsObject()));
            return Task.FromResult(response);
        }
    }
}
