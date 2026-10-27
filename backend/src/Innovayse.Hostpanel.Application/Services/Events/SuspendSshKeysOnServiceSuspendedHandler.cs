using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Events;

/// <summary>
/// When a service is suspended, takes its installed SSH keys off the server they were issued on.
/// Control panels suspend an account by locking its password, which does not stop a key from
/// logging in. The keys are kept, <see cref="SshKeySyncState.Suspended"/>, and put back on
/// unsuspension by <see cref="RestoreSshKeysOnServiceUnsuspendedHandler"/>.
/// </summary>
/// <remarks>
/// Handles <see cref="ClientServiceSuspendedEvent"/>, raised for every suspension whoever suspended
/// the hosting account. A key the agent does not confirm removed stays installed with the reason, so
/// staff can see that it may still log in.
/// </remarks>
public sealed class SuspendSshKeysOnServiceSuspendedHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Changes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="revoker">Changes keys on the server they were issued on.</param>
    public SuspendSshKeysOnServiceSuspendedHandler(IHostpanelDbContext context, ISshKeyRevoker revoker)
    {
        _context = context;
        _revoker = revoker;
    }

    /// <summary>Handles <see cref="ClientServiceSuspendedEvent"/>.</summary>
    /// <param name="evt">The event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every installed key was attempted.</returns>
    public async Task HandleAsync(ClientServiceSuspendedEvent evt, CancellationToken ct)
    {
        var keys = await _context.SshKeys
            .Where(k => k.ClientServiceId == evt.ServiceId && k.SyncState == SshKeySyncState.Installed)
            .ToListAsync(ct);
        if (keys.Count == 0)
        {
            return;
        }

        foreach (var key in keys)
        {
            if (!await _revoker.SuspendOnPinnedServerAsync(key, ct))
            {
                _context.ActivityLogs.Add(ActivityLog.Create(
                    evt.ClientId,
                    $"Could not take SSH key \"{key.Label}\" ({key.Fingerprint}) of suspended service #{evt.ServiceId} off the server; it may still log in: {key.LastSyncError}",
                    null, "system", null, null));
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
