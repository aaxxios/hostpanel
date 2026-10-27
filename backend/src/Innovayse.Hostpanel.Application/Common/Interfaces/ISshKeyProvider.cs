using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// SSH key management capability of one hosting server. Created per server by
/// <see cref="ISshKeyProviderFactory"/>; every call names the account explicitly and the
/// implementation refuses an account whose <see cref="SshAccount.ServerId"/> is not its own.
/// </summary>
public interface ISshKeyProvider
{
    /// <summary>Checks the server's key agent answers, for the admin health check.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The agent's version string.</returns>
    /// <exception cref="SshProviderException">Thrown when the server fails or refuses.</exception>
    Task<string> PingAsync(CancellationToken ct);

    /// <summary>Reports whether SSH access is enabled for the account and how many keys it holds.</summary>
    /// <param name="account">The account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The status.</returns>
    /// <exception cref="SshProviderException">Thrown when the server fails or refuses.</exception>
    Task<SshAccessStatus> GetAccessStatusAsync(SshAccount account, CancellationToken ct);

    /// <summary>Lists the HostPanel-managed keys on the account.</summary>
    /// <param name="account">The account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The keys the server holds.</returns>
    /// <exception cref="SshProviderException">Thrown when the server fails or refuses.</exception>
    Task<IReadOnlyList<InstalledSshKey>> ListKeysAsync(SshAccount account, CancellationToken ct);

    /// <summary>Installs a key. Idempotent for the same <paramref name="keyId"/> and key.</summary>
    /// <param name="account">The account.</param>
    /// <param name="keyId">Stable key id, written into the key comment.</param>
    /// <param name="publicKey">Canonical <c>ssh-ed25519 &lt;base64&gt;</c> line.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the server confirmed the install.</returns>
    /// <exception cref="SshProviderException">Thrown when the server fails or refuses.</exception>
    Task AddKeyAsync(SshAccount account, Guid keyId, string publicKey, CancellationToken ct);

    /// <summary>Removes a key. Idempotent: removing an absent key succeeds.</summary>
    /// <param name="account">The account.</param>
    /// <param name="keyId">Stable key id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the server confirmed the key is gone.</returns>
    /// <exception cref="SshProviderException">Thrown when the server fails or refuses.</exception>
    Task RevokeKeyAsync(SshAccount account, Guid keyId, CancellationToken ct);
}
