using Innovayse.Hostpanel.Domain.Servers;

namespace Innovayse.Hostpanel.Domain.Tests.Servers;

/// <summary>Unit tests for <see cref="ServerSshSettings"/>.</summary>
public sealed class ServerSshSettingsTests
{
    /// <summary>Enabling settings without an agent private key, or with an out-of-range port, is refused.</summary>
    [Fact]
    public void Configure_EnabledButIncomplete_Refused()
    {
        var settings = ServerSshSettings.CreateFor(1);
        Assert.Throws<ArgumentException>(() =>
            settings.Configure(true, "host", 22, "host", 22, "hp", null, "SHA256:x", 5, 5));
        Assert.Throws<ArgumentException>(() =>
            settings.Configure(false, "host", 70000, "host", 22, "hp", null, string.Empty, 5, 5));
    }

    /// <summary>A null private key on a later save keeps the stored key while other fields update.</summary>
    [Fact]
    public void Configure_NullPrivateKey_KeepsStoredOne()
    {
        var settings = ServerSshSettings.CreateFor(1);
        settings.Configure(true, "h", 22, "h", 22, "hp", "KEY", "SHA256:x", 5, 5);
        settings.Configure(true, "h", 2222, "h", 22, "hp", null, "SHA256:x", 5, 5);
        Assert.Equal("KEY", settings.AgentPrivateKey);
        Assert.Equal(2222, settings.SshPort);
    }

    /// <summary>A changed agent endpoint or a new private key clears the last health check; a customer-facing change does not.</summary>
    [Fact]
    public void Configure_ChangedAgentTarget_ClearsHealth()
    {
        var checkedAt = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);
        var settings = ServerSshSettings.CreateFor(1);
        settings.Configure(false, "h", 22, "h", 22, "hp", "KEY", "SHA256:x", 5, 5);
        settings.RecordHealthCheck(true, checkedAt);

        settings.Configure(false, "other", 2222, "h", 22, "hp", null, "SHA256:x", 5, 5);
        Assert.True(settings.IsHealthy);

        settings.Configure(false, "other", 2222, "agent", 22, "hp", null, "SHA256:x", 5, 5);
        Assert.Null(settings.IsHealthy);

        settings.RecordHealthCheck(true, checkedAt);
        settings.Configure(false, "other", 2222, "agent", 22, "hp", "NEW", "SHA256:x", 5, 5);
        Assert.Null(settings.IsHealthy);
        Assert.Equal("NEW", settings.AgentPrivateKey);
    }

    /// <summary>Issuing an identity stores the pair, keeps one binding across rotations and switches access off until tested.</summary>
    [Fact]
    public void IssueIdentity_KeepsBinding_AndDisables()
    {
        var settings = ServerSshSettings.CreateFor(1);
        settings.Configure(true, "h", 22, "h", 22, "hp", "KEY", "SHA256:x", 5, 5);
        settings.RecordHealthCheck(true, new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero));

        settings.IssueIdentity("PRIV1", "ssh-ed25519 AAAA1");
        var binding = settings.AgentBinding;
        Assert.NotNull(binding);
        Assert.False(settings.IsEnabled);
        Assert.Null(settings.IsHealthy);
        Assert.Null(settings.LastHealthCheckAt);
        Assert.Equal("ssh-ed25519 AAAA1", settings.AgentPublicKey);

        settings.IssueIdentity("PRIV2", "ssh-ed25519 AAAA2");
        Assert.Equal(binding, settings.AgentBinding);
        Assert.Equal("PRIV2", settings.AgentPrivateKey);
    }
}
