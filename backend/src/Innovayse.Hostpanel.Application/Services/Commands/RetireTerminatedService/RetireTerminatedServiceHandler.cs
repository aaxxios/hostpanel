using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Commands.RetireTerminatedService;

/// <summary>
/// Closes the open cancellation requests of a terminated service and revokes its SSH keys on the
/// server each was issued on.
/// </summary>
/// <remarks>
/// A terminated hosting account normally takes its home, and the keys, with it; but a termination
/// that did not reach the server would otherwise leave keys that still log in and that no customer
/// can see or revoke any more. So every active key is revoked by its own tag (see
/// <see cref="ISshKeyRevoker"/>), and a key the agent does not confirm removed stays
/// <see cref="SshKeySyncState.RevokeFailed"/> with the reason, visible to staff, rather than being
/// declared gone. Each outcome is written to the client's activity log.
/// </remarks>
public sealed class RetireTerminatedServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Removes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Stages the activity log lines in the same save as the keys.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="revoker">Removes keys on the server they were issued on.</param>
    /// <param name="activity">Stages the activity log lines in the same save as the keys.</param>
    public RetireTerminatedServiceHandler(IHostpanelDbContext context, ISshKeyRevoker revoker, IActivityLogger activity)
    {
        _context = context;
        _revoker = revoker;
        _activity = activity;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The terminated service.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, also when there was nothing left to settle or a key could not be confirmed removed,
    /// or <c>SERVICE_NOT_FOUND</c> when the id names no service, or <c>SERVICE_NOT_TERMINATED</c>
    /// when the service is still running.
    /// </returns>
    public async Task<Result> HandleAsync(RetireTerminatedServiceCommand cmd, CancellationToken ct)
    {
        // Read-only here: only the status and the owner are checked; the requests and keys are what change.
        var service = await _context.ClientServices
            .AsNoTracking()
            .Where(s => s.Id == cmd.ServiceId)
            .Select(s => new { s.ClientId, s.Status })
            .FirstOrDefaultAsync(ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (service.Status is not ServiceStatus.Terminated)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_TERMINATED);
        }

        // Tracked, not AsNoTracking: both are changed and saved below.
        var openRequests = await _context.CancellationRequests
            .Where(r => r.ServiceId == cmd.ServiceId && r.Status == CancellationStatus.Open)
            .ToListAsync(ct);
        var liveKeys = await _context.SshKeys
            .Where(k => k.ClientServiceId == cmd.ServiceId && k.SyncState != SshKeySyncState.Revoked)
            .ToListAsync(ct);

        foreach (var request in openRequests)
        {
            request.Close();
        }

        foreach (var key in liveKeys)
        {
            var revoked = await _revoker.RevokeOnPinnedServerAsync(key, ct);
            await _activity.RecordAsync(
                new ActivityLogEntry(
                    service.ClientId,
                    revoked
                        ? $"Revoked SSH key \"{key.Label}\" ({key.Fingerprint}) of terminated service #{cmd.ServiceId}"
                        : $"Could not revoke SSH key \"{key.Label}\" ({key.Fingerprint}) of terminated service #{cmd.ServiceId}: {key.LastSyncError}"),
                ct);
        }

        if (openRequests.Count > 0 || liveKeys.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
