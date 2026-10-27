namespace Innovayse.Hostpanel.Domain.Ssh.Enums;

/// <summary>
/// Where an <see cref="SshKey"/> stands relative to the hosting server it was issued for.
/// </summary>
/// <remarks>
/// The server's <c>authorized_keys</c> file is the source of truth for what can log in. This
/// state records what HostPanel last <i>observed</i> there, so a provider failure is never
/// mistaken for success: a key is only <see cref="Installed"/> once the provider confirmed it,
/// and only <see cref="Revoked"/> once the provider confirmed its removal.
/// </remarks>
public enum SshKeySyncState
{
    /// <summary>Recorded; the provider has not yet confirmed the install.</summary>
    Pending,

    /// <summary>The provider confirmed the key is in the account's authorized keys.</summary>
    Installed,

    /// <summary>The install call failed or timed out; the key may or may not be on the server.</summary>
    InstallFailed,

    /// <summary>The revoke call failed or timed out; the key may still be able to log in.</summary>
    RevokeFailed,

    /// <summary>The provider confirmed the key is gone from the server. Terminal.</summary>
    Revoked,
}
