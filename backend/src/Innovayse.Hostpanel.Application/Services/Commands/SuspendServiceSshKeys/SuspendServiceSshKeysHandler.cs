using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.SuspendServiceSshKeys;

/// <summary>
/// Takes a suspended service's installed SSH keys off their pinned server. The keys are kept,
/// <see cref="SshKeySyncState.Suspended"/>, and put back on unsuspension. A key the agent does not
/// confirm removed stays installed with the reason, and an activity log line tells staff it may
/// still log in.
/// </summary>
public sealed class SuspendServiceSshKeysHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Changes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Stages the activity log lines in the same save as the keys.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="revoker">Changes keys on the server they were issued on.</param>
    /// <param name="activity">Stages the activity log lines in the same save as the keys.</param>
    public SuspendServiceSshKeysHandler(IHostpanelDbContext context, ISshKeyRevoker revoker, IActivityLogger activity)
    {
        _context = context;
        _revoker = revoker;
        _activity = activity;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The suspended service.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once every installed key was attempted, also when there was none;
    /// <c>SERVICE_NOT_FOUND</c> when the id names no service; <c>SERVICE_NOT_SUSPENDED</c> when the
    /// service is not suspended any more.
    /// </returns>
    public async Task<Result> HandleAsync(SuspendServiceSshKeysCommand cmd, CancellationToken ct)
    {
        // Read-only: only the status and the owner are needed; the keys are what change.
        var service = await _context.ClientServices
            .AsNoTracking()
            .Where(s => s.Id == cmd.ServiceId)
            .Select(s => new { s.ClientId, s.Status })
            .FirstOrDefaultAsync(ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (service.Status is not ServiceStatus.Suspended)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_SUSPENDED);
        }

        var keys = await _context.SshKeys
            .Where(k => k.ClientServiceId == cmd.ServiceId && k.SyncState == SshKeySyncState.Installed)
            .ToListAsync(ct);
        if (keys.Count == 0)
        {
            return Result.Success();
        }

        foreach (var key in keys)
        {
            if (!await _revoker.SuspendOnPinnedServerAsync(key, ct))
            {
                await _activity.RecordAsync(
                    new ActivityLogEntry(
                        service.ClientId,
                        $"Could not take SSH key \"{key.Label}\" ({key.Fingerprint}) of suspended service #{cmd.ServiceId} off the server; it may still log in: {key.LastSyncError}"),
                    ct);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
