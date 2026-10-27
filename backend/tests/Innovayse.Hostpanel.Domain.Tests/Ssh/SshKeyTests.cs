using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Ssh;

/// <summary>State and server-binding rules of <see cref="SshKey"/>.</summary>
public sealed class SshKeyTests
{
    private static SshKey NewKey() => SshKey.Create(10, 1, "web", " laptop ", "ssh-ed25519 AAAA", "SHA256:x");

    [Fact]
    public void Create_IsPendingWithStableIdAndTrimmedLabel()
    {
        var key = NewKey();
        Assert.Equal(SshKeySyncState.Pending, key.SyncState);
        Assert.NotEqual(Guid.Empty, key.KeyId);
        Assert.Equal("laptop", key.Label);
    }

    [Theory]
    [InlineData(1, "web", true)]
    [InlineData(2, "web", false)]
    [InlineData(1, "Web", false)]
    [InlineData(null, "web", false)]
    public void IsBoundTo_RequiresSameServerAndAccount(int? serverId, string account, bool expected) =>
        Assert.Equal(expected, NewKey().IsBoundTo(serverId, account));

    [Fact]
    public void Revoked_IsTerminal()
    {
        var key = NewKey();
        key.MarkInstalled();
        key.MarkRevoked();
        Assert.False(key.IsActive);
        Assert.NotNull(key.RevokedAt);
        Assert.Throws<InvalidOperationException>(() => key.MarkInstalled());
        Assert.Throws<InvalidOperationException>(() => key.MarkRevokeFailed("x"));
    }

    [Fact]
    public void RevokeFailed_StaysActive()
    {
        var key = NewKey();
        key.MarkInstalled();
        key.MarkRevokeFailed("unreachable");
        Assert.True(key.IsActive);
        Assert.Equal("unreachable", key.LastSyncError);
    }

    [Fact]
    public void ServerSettings_EnabledButIncomplete_Refused()
    {
        var settings = ServerSshSettings.CreateFor(1);
        Assert.Throws<ArgumentException>(() =>
            settings.Configure(true, "host", 22, "host", 22, "hp", null, "SHA256:x", 5, 5));
        Assert.Throws<ArgumentException>(() =>
            settings.Configure(false, "host", 70000, "host", 22, "hp", null, string.Empty, 5, 5));
    }

    [Fact]
    public void ServerSettings_NullPrivateKey_KeepsStoredOne()
    {
        var settings = ServerSshSettings.CreateFor(1);
        settings.Configure(true, "h", 22, "h", 22, "hp", "KEY", "SHA256:x", 5, 5);
        settings.Configure(true, "h", 2222, "h", 22, "hp", null, "SHA256:x", 5, 5);
        Assert.Equal("KEY", settings.AgentPrivateKey);
        Assert.Equal(2222, settings.SshPort);
    }
}
