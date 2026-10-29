using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Ssh;

/// <summary>State and server-binding rules of <see cref="SshKey"/>.</summary>
public sealed class SshKeyTests
{
    /// <summary>Creates a key for account <c>web</c> on server 1 with a padded label.</summary>
    /// <returns>A fresh pending key.</returns>
    private static SshKey NewKey() => SshKey.Create(10, 1, "web", " laptop ", "ssh-ed25519 AAAA", "SHA256:x");

    /// <summary>A new key is pending, has a stable key id and a trimmed label.</summary>
    [Fact]
    public void Create_IsPendingWithStableIdAndTrimmedLabel()
    {
        var key = NewKey();
        Assert.Equal(SshKeySyncState.Pending, key.SyncState);
        Assert.NotEqual(Guid.Empty, key.KeyId);
        Assert.Equal("laptop", key.Label);
    }

    /// <summary>A key is bound only to the exact server and case-sensitive account it was created for.</summary>
    /// <param name="serverId">The server asked about.</param>
    /// <param name="account">The account asked about.</param>
    /// <param name="expected">Whether the key should be bound.</param>
    [Theory]
    [InlineData(1, "web", true)]
    [InlineData(2, "web", false)]
    [InlineData(1, "Web", false)]
    [InlineData(null, "web", false)]
    public void IsBoundTo_RequiresSameServerAndAccount(int? serverId, string account, bool expected) =>
        Assert.Equal(expected, NewKey().IsBoundTo(serverId, account));

    /// <summary>A revoked key is inactive and accepts no further state change.</summary>
    [Fact]
    public void Revoked_IsTerminal()
    {
        var key = NewKey();
        key.MarkInstalled();
        var revokedAt = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);
        key.MarkRevoked(revokedAt);
        Assert.False(key.IsActive);
        Assert.Equal(revokedAt, key.RevokedAt);
        Assert.Throws<InvalidOperationException>(() => key.MarkInstalled());
        Assert.Throws<InvalidOperationException>(() => key.MarkRevokeFailed(SshProviderFailure.Unreachable));
        Assert.Throws<InvalidOperationException>(() => key.MarkSuspended());
        Assert.Throws<InvalidOperationException>(() => key.RecordSyncError(SshProviderFailure.Timeout));
    }

    /// <summary>A failed revoke keeps the key active and records the error.</summary>
    [Fact]
    public void RevokeFailed_StaysActive()
    {
        var key = NewKey();
        key.MarkInstalled();
        key.MarkRevokeFailed(SshProviderFailure.Unreachable);
        Assert.True(key.IsActive);
        Assert.Equal(SshProviderFailure.Unreachable, key.LastSyncError);
    }

    /// <summary>A failed install records why; a later confirmed install clears it.</summary>
    [Fact]
    public void InstallFailed_RecordsTheFailure_AndInstallClearsIt()
    {
        var key = NewKey();
        key.MarkInstallFailed(SshProviderFailure.Timeout);
        Assert.Equal(SshKeySyncState.InstallFailed, key.SyncState);
        Assert.Equal(SshProviderFailure.Timeout, key.LastSyncError);

        key.MarkInstalled();
        Assert.Null(key.LastSyncError);
    }

    /// <summary>A key taken off for a suspension stays active, clears the last error and can be installed again.</summary>
    [Fact]
    public void Suspended_StaysActive_AndCanBeRestored()
    {
        var key = NewKey();
        key.MarkInstallFailed(SshProviderFailure.Timeout);
        key.MarkSuspended();
        Assert.Equal(SshKeySyncState.Suspended, key.SyncState);
        Assert.True(key.IsActive);
        Assert.Null(key.LastSyncError);

        key.MarkInstalled();
        Assert.Equal(SshKeySyncState.Installed, key.SyncState);
    }

    /// <summary>A failed call recorded without a transition keeps the state and stores why it failed.</summary>
    [Fact]
    public void RecordSyncError_KeepsTheState()
    {
        var key = NewKey();
        key.MarkInstalled();
        key.RecordSyncError(SshProviderFailure.Unreachable);
        Assert.Equal(SshKeySyncState.Installed, key.SyncState);
        Assert.Equal(SshProviderFailure.Unreachable, key.LastSyncError);
    }
}
