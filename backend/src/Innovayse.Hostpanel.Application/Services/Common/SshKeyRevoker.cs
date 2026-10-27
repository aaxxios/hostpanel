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
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == key.ServerId, ct);
        var settings = server is null
            ? null
            : await _context.ServerSshSettings.AsNoTracking().FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);

        // A disabled server is still reachable for removals: switching the feature off must not
        // strand keys that can log in. Only a server with no usable agent settings cannot be asked.
        var provider = server is not null && settings is not null && settings.IsComplete
            ? _factory.CreateFor(server, settings)
            : null;
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
}
