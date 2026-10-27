using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Services.Commands.RevokeMySshKey;

/// <summary>
/// Removes a key from the server it was installed on, and only from that server.
/// </summary>
/// <remarks>
/// The key's pinned server and account must still be the service's. If the service has moved,
/// nothing is sent anywhere: the new server never held this key, and the old one may now host a
/// different customer under the same username. Revocation is allowed while the account's shell is
/// disabled — removing access must never depend on access being enabled.
/// </remarks>
public sealed class RevokeMySshKeyHandler
{
    /// <summary>The rule that says a client may only touch their own services.</summary>
    private readonly IServiceOwnership _ownership;

    /// <summary>Resolves server, provider and account once ownership is settled.</summary>
    private readonly ISshServiceContextResolver _resolver;

    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking, for the audit entry.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Logger.</summary>
    private readonly ILogger<RevokeMySshKeyHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own services.</param>
    /// <param name="resolver">Resolves server, provider and account once ownership is settled.</param>
    /// <param name="context">Data context.</param>
    /// <param name="caller">Who is asking, for the audit entry.</param>
    /// <param name="logger">Logger.</param>
    public RevokeMySshKeyHandler(
        IServiceOwnership ownership, ISshServiceContextResolver resolver, IHostpanelDbContext context,
        ICurrentUserService caller, ILogger<RevokeMySshKeyHandler> logger)
    {
        _ownership = ownership;
        _resolver = resolver;
        _context = context;
        _caller = caller;
        _logger = logger;
    }

    /// <summary>Handles <see cref="RevokeMySshKeyCommand"/>.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the server confirmed the removal, or one of <c>SERVICE_NOT_FOUND</c>,
    /// <c>SSH_KEY_NOT_FOUND</c>, <c>SSH_ACCESS_UNAVAILABLE</c>, <c>SSH_KEY_STALE</c>,
    /// <c>SSH_SERVER_UNAVAILABLE</c>.
    /// </returns>
    public async Task<Result> HandleAsync(RevokeMySshKeyCommand cmd, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(cmd.ServiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return ownershipOutcome;
        }

        var context = await _resolver.ResolveAsync(cmd.ServiceId, ct);
        if (context is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        var key = await _context.SshKeys.FirstOrDefaultAsync(
            k => k.ClientServiceId == context.Service.Id && k.KeyId == cmd.KeyId, ct);
        if (key is null || !key.IsActive)
        {
            return Result.FailureCode(HostpanelErrorCodes.SSH_KEY_NOT_FOUND);
        }

        // Shell-disabled accounts still resolve as Available here: the shell check is the
        // server's, asked only by the access query, so revocation never depends on it.
        if (context.State is not SshAccessState.Available)
        {
            return Result.FailureCode(HostpanelErrorCodes.SSH_ACCESS_UNAVAILABLE);
        }

        var account = context.Account!;
        if (!key.IsBoundTo(account.ServerId, account.Username))
        {
            return Result.FailureCode(HostpanelErrorCodes.SSH_KEY_STALE);
        }

        try
        {
            await context.Provider!.RevokeKeyAsync(account, key.KeyId, ct);
            key.MarkRevoked();
        }
        catch (SshProviderException ex)
        {
            _logger.LogWarning(ex, "SSH key {KeyId} revoke on server {ServerId} failed: {Code}",
                key.KeyId, account.ServerId, ex.Code);
            key.MarkRevokeFailed(ex.Code);
            await _context.SaveChangesAsync(ct);
            return Result.FailureCode(HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE);
        }

        _context.ActivityLogs.Add(ActivityLog.Create(
            context.Service.ClientId,
            $"Revoked SSH key \"{key.Label}\" ({key.Fingerprint}) from service #{context.Service.Id}",
            _caller.UserId, _caller.UserName, _caller.Email, _caller.IpAddress));
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
