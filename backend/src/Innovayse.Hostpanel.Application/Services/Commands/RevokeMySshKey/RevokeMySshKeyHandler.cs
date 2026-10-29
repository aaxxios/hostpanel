using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Services.Commands.RevokeMySshKey;

/// <summary>
/// Removes a key from the server it was installed on, and only from that server.
/// </summary>
/// <remarks>
/// A key that still belongs to the service's current server and account is removed through the
/// normal provider. Otherwise (the service moved, or SSH key management was switched off on its
/// server) it is removed on the server and account it was issued on, by its own tag only (see
/// <see cref="ISshKeyRevoker"/>); the service's current server is never asked about a key it never
/// held. Revocation is allowed while the account's shell is disabled: removing access must never
/// depend on access being enabled.
/// </remarks>
public sealed class RevokeMySshKeyHandler
{
    /// <summary>The rule that says a client may only touch their own services.</summary>
    private readonly IServiceOwnership _ownership;

    /// <summary>Resolves server, provider and account once ownership is settled.</summary>
    private readonly ISshServiceContextResolver _resolver;

    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Stages the activity log line in the same save as the revocation.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Removes keys on the server they were issued on, when the service no longer points there.</summary>
    private readonly ISshKeyRevoker _revoker;

    /// <summary>Logger.</summary>
    private readonly ILogger<RevokeMySshKeyHandler> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initialises the handler.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own services.</param>
    /// <param name="resolver">Resolves server, provider and account once ownership is settled.</param>
    /// <param name="context">Data context.</param>
    /// <param name="activity">Stages the activity log line in the same save as the revocation.</param>
    /// <param name="revoker">Removes keys on the server they were issued on.</param>
    /// <param name="logger">Records a provider failure without putting its detail on the wire.</param>
    /// <param name="clock">The source of the current moment.</param>
    public RevokeMySshKeyHandler(
        IServiceOwnership ownership, ISshServiceContextResolver resolver, IHostpanelDbContext context,
        IActivityLogger activity, ISshKeyRevoker revoker, ILogger<RevokeMySshKeyHandler> logger, IClock clock)
    {
        _ownership = ownership;
        _resolver = resolver;
        _context = context;
        _activity = activity;
        _revoker = revoker;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>Handles <see cref="RevokeMySshKeyCommand"/>.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the server confirmed the removal, or one of <c>SERVICE_NOT_FOUND</c>,
    /// <c>SSH_KEY_NOT_FOUND</c>, <c>SSH_ACCESS_UNAVAILABLE</c> (service not active),
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

        if (context.State is SshAccessState.ServiceInactive)
        {
            return Result.FailureCode(HostpanelErrorCodes.SSH_ACCESS_UNAVAILABLE);
        }

        var revoked = context.State is SshAccessState.Available && key.IsBoundTo(context.Account?.ServerId, context.Account?.Username)
            ? await RevokeOnCurrentServerAsync(context.Provider!, context.Account!, key, ct)
            : await _revoker.RevokeOnPinnedServerAsync(key, ct);
        if (!revoked)
        {
            // The key records why; it stays visible as possibly still able to log in.
            await _context.SaveChangesAsync(ct);
            return Result.FailureCode(HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE);
        }

        await _activity.RecordAsync(
            new ActivityLogEntry(
                context.Service.ClientId,
                $"Revoked SSH key \"{key.Label}\" ({key.Fingerprint}) from service #{context.Service.Id}"),
            ct);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>
    /// Revokes a key that still belongs to the service's current server and account through that
    /// server's provider. Shell-disabled accounts still resolve as available: the shell check is the
    /// server's, asked only by the access query, so revocation never depends on it.
    /// </summary>
    /// <param name="provider">The current server's provider.</param>
    /// <param name="account">The service's current account.</param>
    /// <param name="key">The tracked key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when the server confirmed the key is gone; the key records the outcome either way.</returns>
    private async Task<bool> RevokeOnCurrentServerAsync(ISshKeyProvider provider, SshAccount account, SshKey key, CancellationToken ct)
    {
        try
        {
            await provider.RevokeKeyAsync(account, key.KeyId, ct);
            key.MarkRevoked(_clock.UtcNow);
            return true;
        }
        catch (SshProviderException ex)
        {
            _logger.LogWarning(ex, "SSH key {KeyId} revoke on server {ServerId} failed: {Failure}",
                key.KeyId, account.ServerId, ex.Failure);
            key.MarkRevokeFailed(ex.Failure);
            return false;
        }
    }
}
