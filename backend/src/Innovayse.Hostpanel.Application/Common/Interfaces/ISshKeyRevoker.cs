using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Removes a HostPanel-managed key from the server and account it was <b>issued on</b> (its pinned
/// <see cref="SshKey.ServerId"/> and <see cref="SshKey.AccountUsername"/>), not from wherever the
/// service lives now. Used when the normal customer path cannot reach it: the service moved, SSH
/// key management was switched off on the server, the service was terminated, or an administrator
/// clears a server before uninstalling its agent.
/// </summary>
/// <remarks>
/// Safe on a server whose account now belongs to someone else: the agent removes only the line
/// tagged with this key's id, which only HostPanel wrote. Keys added by any other means are never
/// touched. The key is marked revoked only when the agent confirmed it; any doubt leaves it
/// <see cref="Domain.Ssh.Enums.SshKeySyncState.RevokeFailed"/> with the reason, so nothing that
/// might still log in disappears from view.
/// </remarks>
public interface ISshKeyRevoker
{
    /// <summary>Revokes the key on its pinned server and records the outcome on the key. Does not save.</summary>
    /// <param name="key">A tracked, active key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when the agent confirmed the key is gone.</returns>
    public Task<bool> RevokeOnPinnedServerAsync(SshKey key, CancellationToken ct);

    /// <summary>
    /// Takes an installed key off its pinned server for a suspension and marks it
    /// <see cref="Domain.Ssh.Enums.SshKeySyncState.Suspended"/>. When the agent does not confirm, the
    /// key stays installed with the reason recorded. Does not save.
    /// </summary>
    /// <param name="key">A tracked, installed key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when the agent confirmed the key is gone.</returns>
    public Task<bool> SuspendOnPinnedServerAsync(SshKey key, CancellationToken ct);

    /// <summary>
    /// Puts a suspended key back on its pinned server, under the same id. Marks it installed, or
    /// <see cref="Domain.Ssh.Enums.SshKeySyncState.InstallFailed"/> with the reason. Does not save.
    /// </summary>
    /// <param name="key">A tracked, suspended key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when the agent confirmed the key is back.</returns>
    public Task<bool> RestoreOnPinnedServerAsync(SshKey key, CancellationToken ct);
}
