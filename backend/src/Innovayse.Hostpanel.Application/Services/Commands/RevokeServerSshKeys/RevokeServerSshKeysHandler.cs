using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshTarget;
using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Commands.RevokeServerSshKeys;

/// <summary>
/// Handles <see cref="RevokeServerSshKeysCommand"/>: every active key pinned to the server is revoked
/// there, whatever server its service is on now, and each outcome is written to the owner's activity log.
/// </summary>
public sealed class RevokeServerSshKeysHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Message bus, for the server lookup owned by the servers module.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Removes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Stages the activity log lines in the same save as the keys.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>The administrator, for the log line.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Logger; also the admin audit trail.</summary>
    private readonly ILogger<RevokeServerSshKeysHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Message bus, for the server lookup owned by the servers module.</param>
    /// <param name="revoker">Removes keys on the server they were issued on.</param>
    /// <param name="activity">Stages the activity log lines in the same save as the keys.</param>
    /// <param name="caller">The administrator, for the log line.</param>
    /// <param name="logger">Logger; also the admin audit trail.</param>
    public RevokeServerSshKeysHandler(
        IHostpanelDbContext context, IMessageBus bus, ISshKeyRevoker revoker, IActivityLogger activity,
        ICurrentUserService caller, ILogger<RevokeServerSshKeysHandler> logger)
    {
        _context = context;
        _bus = bus;
        _revoker = revoker;
        _activity = activity;
        _caller = caller;
        _logger = logger;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many keys were removed and how many could not be confirmed, or <c>SERVER_NOT_FOUND</c>.</returns>
    public async Task<Result<RevokeServerSshKeysResultDto>> HandleAsync(RevokeServerSshKeysCommand cmd, CancellationToken ct)
    {
        var server = await _bus.InvokeAsync<Result<ServerSshTargetDto>>(new GetServerSshTargetQuery(cmd.ServerId), ct);
        if (server.IsFailure)
        {
            // A failed Result always carries its code.
            return Result<RevokeServerSshKeysResultDto>.FailureCode(server.ErrorCode!);
        }

        // Keys pinned to this server, whatever server their service is on now.
        var keys = await _context.SshKeys
            .Where(k => k.ServerId == cmd.ServerId && k.SyncState != SshKeySyncState.Revoked)
            .ToListAsync(ct);
        var serviceIds = keys.Select(k => k.ClientServiceId).Distinct().ToList();
        var owners = await _context.ClientServices
            .AsNoTracking()
            .Where(s => serviceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.ClientId, ct);

        var revoked = 0;
        var failed = 0;
        foreach (var key in keys)
        {
            var confirmed = await _revoker.RevokeOnPinnedServerAsync(key, ct);
            if (confirmed)
            {
                revoked++;
            }
            else
            {
                failed++;
            }

            if (owners.TryGetValue(key.ClientServiceId, out var clientId))
            {
                await _activity.RecordAsync(
                    new ActivityLogEntry(
                        clientId,
                        confirmed
                            ? $"Administrator revoked SSH key \"{key.Label}\" ({key.Fingerprint}) on server #{cmd.ServerId}"
                            : $"Administrator could not revoke SSH key \"{key.Label}\" ({key.Fingerprint}) on server #{cmd.ServerId}: {key.LastSyncError}"),
                    ct);
            }
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation(
            "SSH keys on server {ServerId} revoked by user {UserId}: {Revoked} removed, {Failed} unconfirmed",
            cmd.ServerId, _caller.UserId, revoked, failed);
        return Result<RevokeServerSshKeysResultDto>.Success(new RevokeServerSshKeysResultDto(revoked, failed));
    }
}
