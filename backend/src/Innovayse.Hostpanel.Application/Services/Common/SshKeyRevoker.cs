using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>Default <see cref="ISshKeyRevoker"/>.</summary>
public sealed class SshKeyRevoker : ISshKeyRevoker
{
    /// <summary>Reason recorded when the pinned server has no usable agent configuration.</summary>
    public const string AgentNotConfigured = "agent_not_configured";

    /// <summary>The agent's refusal for an account the panel still has locked.</summary>
    public const string AccountStillSuspended = "account_suspended";

    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Logger.</summary>
    private readonly ILogger<SshKeyRevoker> _logger;

    /// <summary>Initialises the revoker.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory.</param>
    /// <param name="logger">Logger.</param>
    public SshKeyRevoker(IHostpanelDbContext context, ISshKeyProviderFactory factory, ILogger<SshKeyRevoker> logger)
    {
        _context = context;
        _factory = factory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeOnPinnedServerAsync(SshKey key, CancellationToken ct)
    {
        var provider = await ProviderForAsync(key, ct);
        if (provider is null)
        {
            key.MarkRevokeFailed(AgentNotConfigured);
            _logger.LogWarning("SSH key {KeyId} could not be revoked: server {ServerId} has no usable agent.", key.KeyId, key.ServerId);
            return false;
        }

        try
        {
            await provider.RevokeKeyAsync(new SshAccount(key.ServerId, key.AccountUsername), key.KeyId, ct);
            key.MarkRevoked();
            return true;
        }
        catch (SshProviderException ex)
        {
            // account_not_found included: the account may be gone, or only gone from the panel's
            // list while its home (and the key) remain. Not confirmed, so not marked revoked.
            key.MarkRevokeFailed(ex.Code);
            _logger.LogWarning(ex, "SSH key {KeyId} revoke on pinned server {ServerId} failed: {Code}", key.KeyId, key.ServerId, ex.Code);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SuspendOnPinnedServerAsync(SshKey key, CancellationToken ct)
    {
        var provider = await ProviderForAsync(key, ct);
        if (provider is null)
        {
            key.RecordSyncError(AgentNotConfigured);
            _logger.LogWarning("SSH key {KeyId} could not be suspended: server {ServerId} has no usable agent.", key.KeyId, key.ServerId);
            return false;
        }

        try
        {
            await provider.RevokeKeyAsync(new SshAccount(key.ServerId, key.AccountUsername), key.KeyId, ct);
            key.MarkSuspended();
            return true;
        }
        catch (SshProviderException ex)
        {
            // Not confirmed gone, so it may still log in: it stays installed, with the reason.
            key.RecordSyncError(ex.Code);
            _logger.LogWarning(ex, "SSH key {KeyId} suspension on pinned server {ServerId} failed: {Code}", key.KeyId, key.ServerId, ex.Code);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RestoreOnPinnedServerAsync(SshKey key, CancellationToken ct)
    {
        var provider = await ProviderForAsync(key, ct);
        if (provider is null)
        {
            key.MarkInstallFailed(AgentNotConfigured);
            _logger.LogWarning("SSH key {KeyId} could not be restored: server {ServerId} has no usable agent.", key.KeyId, key.ServerId);
            return false;
        }

        try
        {
            await provider.AddKeyAsync(new SshAccount(key.ServerId, key.AccountUsername), key.KeyId, key.PublicKey, ct);
            key.MarkInstalled();
            return true;
        }
        catch (SshProviderException ex) when (ex.Code == AccountStillSuspended)
        {
            // The panel has not unlocked the account yet; RestoreSuspendedSshKeys retries once it has.
            key.RecordSyncError(ex.Code);
            _logger.LogInformation("SSH key {KeyId} not restored yet: account on server {ServerId} is still suspended.", key.KeyId, key.ServerId);
            return false;
        }
        catch (SshProviderException ex)
        {
            key.MarkInstallFailed(ex.Code);
            _logger.LogWarning(ex, "SSH key {KeyId} restore on pinned server {ServerId} failed: {Code}", key.KeyId, key.ServerId, ex.Code);
            return false;
        }
    }

    /// <summary>
    /// The agent of the server the key was issued on. A disabled server is still reachable: switching
    /// the feature off must not strand keys that can log in. Only a server with no usable agent
    /// settings cannot be asked.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The provider, or null when the pinned server has no usable agent.</returns>
    private async Task<ISshKeyProvider?> ProviderForAsync(SshKey key, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == key.ServerId, ct);
        var settings = server is null
            ? null
            : await _context.ServerSshSettings.AsNoTracking().FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);
        return server is not null && settings is not null && settings.IsComplete
            ? _factory.CreateFor(server, settings)
            : null;
    }
}
