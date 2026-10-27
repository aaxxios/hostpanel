using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Events;

/// <summary>
/// When a service is unsuspended, puts back the SSH keys <see cref="SuspendSshKeysOnServiceSuspendedHandler"/>
/// took off, on the server each was issued on and under the same id.
/// </summary>
/// <remarks>
/// Runs on <see cref="ClientServiceUnsuspendedEvent"/> and again on <see cref="RestoreSuspendedSshKeys"/>,
/// sent once the panel has unlocked the account: the two unsuspensions are not ordered, and the agent
/// refuses keys for a locked account. That refusal leaves the key suspended for the second pass; any
/// other failure makes it <see cref="SshKeySyncState.InstallFailed"/> with the reason, and the
/// customer can remove it and add it again.
/// </remarks>
public sealed class RestoreSshKeysOnServiceUnsuspendedHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Changes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="revoker">Changes keys on the server they were issued on.</param>
    public RestoreSshKeysOnServiceUnsuspendedHandler(IHostpanelDbContext context, ISshKeyRevoker revoker)
    {
        _context = context;
        _revoker = revoker;
    }

    /// <summary>Handles <see cref="ClientServiceUnsuspendedEvent"/>.</summary>
    /// <param name="evt">The event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every suspended key was attempted.</returns>
    public Task HandleAsync(ClientServiceUnsuspendedEvent evt, CancellationToken ct) =>
        RestoreAsync(evt.ServiceId, evt.ClientId, ct);

    /// <summary>Handles <see cref="RestoreSuspendedSshKeys"/>.</summary>
    /// <param name="msg">The message.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every suspended key was attempted.</returns>
    public Task HandleAsync(RestoreSuspendedSshKeys msg, CancellationToken ct) =>
        RestoreAsync(msg.ServiceId, msg.ClientId, ct);

    /// <summary>Puts back every suspended key of the service.</summary>
    /// <param name="serviceId">The service.</param>
    /// <param name="clientId">Its owner, for the activity log.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every suspended key was attempted.</returns>
    private async Task RestoreAsync(int serviceId, int clientId, CancellationToken ct)
    {
        var keys = await _context.SshKeys
            .Where(k => k.ClientServiceId == serviceId && k.SyncState == SshKeySyncState.Suspended)
            .ToListAsync(ct);
        if (keys.Count == 0)
        {
            return;
        }

        foreach (var key in keys)
        {
            if (!await _revoker.RestoreOnPinnedServerAsync(key, ct) && key.SyncState is SshKeySyncState.InstallFailed)
            {
                _context.ActivityLogs.Add(ActivityLog.Create(
                    clientId,
                    $"Could not restore SSH key \"{key.Label}\" ({key.Fingerprint}) of unsuspended service #{serviceId}: {key.LastSyncError}",
                    null, "system", null, null));
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
