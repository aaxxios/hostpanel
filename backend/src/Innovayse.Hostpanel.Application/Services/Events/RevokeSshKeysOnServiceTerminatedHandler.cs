using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Events;

/// <summary>
/// When a service is terminated, removes every SSH key HostPanel installed for it, on the server
/// each key was issued on. A terminated hosting account normally takes its home — and the keys —
/// with it, but a termination that did not reach the server would otherwise leave keys that still
/// log in and that no customer can see or revoke any more.
/// </summary>
/// <remarks>
/// Idempotent: only keys that are still active are touched, and the agent's revoke is idempotent
/// by key id. Keys the agent cannot confirm stay <see cref="SshKeySyncState.RevokeFailed"/> with the
/// reason, visible to administrators, rather than being declared gone.
/// </remarks>
public sealed class RevokeSshKeysOnServiceTerminatedHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Removes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="revoker">Removes keys on the server they were issued on.</param>
    public RevokeSshKeysOnServiceTerminatedHandler(IHostpanelDbContext context, ISshKeyRevoker revoker)
    {
        _context = context;
        _revoker = revoker;
    }

    /// <summary>Handles <see cref="ServiceTerminatedEvent"/>.</summary>
    /// <param name="evt">The event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every active key was attempted.</returns>
    public async Task HandleAsync(ServiceTerminatedEvent evt, CancellationToken ct)
    {
        var keys = await _context.SshKeys
            .Where(k => k.ClientServiceId == evt.ServiceId && k.SyncState != SshKeySyncState.Revoked)
            .ToListAsync(ct);
        if (keys.Count == 0)
        {
            return;
        }

        foreach (var key in keys)
        {
            var revoked = await _revoker.RevokeOnPinnedServerAsync(key, ct);
            _context.ActivityLogs.Add(ActivityLog.Create(
                evt.ClientId,
                revoked
                    ? $"Revoked SSH key \"{key.Label}\" ({key.Fingerprint}) of terminated service #{evt.ServiceId}"
                    : $"Could not revoke SSH key \"{key.Label}\" ({key.Fingerprint}) of terminated service #{evt.ServiceId}: {key.LastSyncError}",
                null, "system", null, null));
        }

        await _context.SaveChangesAsync(ct);
    }
}
