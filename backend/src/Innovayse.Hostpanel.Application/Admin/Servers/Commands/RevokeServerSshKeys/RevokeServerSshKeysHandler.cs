using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.RevokeServerSshKeys;

/// <summary>Handles <see cref="RevokeServerSshKeysCommand"/>.</summary>
public sealed class RevokeServerSshKeysHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Removes keys on the server they were issued on.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>The administrator, for the audit trail.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Logger; also the admin audit trail.</summary>
    private readonly ILogger<RevokeServerSshKeysHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="revoker">Removes keys on the server they were issued on.</param>
    /// <param name="caller">The administrator, for the audit trail.</param>
    /// <param name="logger">Logger; also the admin audit trail.</param>
    public RevokeServerSshKeysHandler(
        IHostpanelDbContext context, ISshKeyRevoker revoker, ICurrentUserService caller, ILogger<RevokeServerSshKeysHandler> logger)
    {
        _context = context;
        _revoker = revoker;
        _caller = caller;
        _logger = logger;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many keys were removed and how many could not be confirmed, or <c>SERVER_NOT_FOUND</c>.</returns>
    public async Task<Result<RevokeServerSshKeysResultDto>> HandleAsync(RevokeServerSshKeysCommand cmd, CancellationToken ct)
    {
        if (!await _context.Servers.AnyAsync(s => s.Id == cmd.ServerId, ct))
        {
            return Result<RevokeServerSshKeysResultDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        // Keys pinned to this server, whatever server their service is on now.
        var keys = await _context.SshKeys
            .Where(k => k.ServerId == cmd.ServerId && k.SyncState != SshKeySyncState.Revoked)
            .ToListAsync(ct);
        var clients = await _context.ClientServices
            .Where(s => keys.Select(k => k.ClientServiceId).Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.ClientId, ct);

        int revoked = 0, failed = 0;
        foreach (var key in keys)
        {
            var ok = await _revoker.RevokeOnPinnedServerAsync(key, ct);
            if (ok)
            {
                revoked++;
            }
            else
            {
                failed++;
            }

            if (clients.TryGetValue(key.ClientServiceId, out var clientId))
            {
                _context.ActivityLogs.Add(ActivityLog.Create(
                    clientId,
                    ok
                        ? $"Administrator revoked SSH key \"{key.Label}\" ({key.Fingerprint}) on server #{cmd.ServerId}"
                        : $"Administrator could not revoke SSH key \"{key.Label}\" ({key.Fingerprint}) on server #{cmd.ServerId}: {key.LastSyncError}",
                    _caller.UserId, _caller.UserName, _caller.Email, _caller.IpAddress));
            }
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation(
            "SSH keys on server {ServerId} revoked by user {UserId}: {Revoked} removed, {Failed} unconfirmed",
            cmd.ServerId, _caller.UserId, revoked, failed);
        return Result<RevokeServerSshKeysResultDto>.Success(new RevokeServerSshKeysResultDto(revoked, failed));
    }
}
