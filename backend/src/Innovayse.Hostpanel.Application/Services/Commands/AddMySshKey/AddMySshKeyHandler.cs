using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Application.Services.Extensions;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Services.Commands.AddMySshKey;

/// <summary>
/// Validates a public key and installs it on the server the caller's service is assigned to.
/// </summary>
/// <remarks>
/// The record is saved as <see cref="SshKeySyncState.Pending"/> <b>before</b> the server is
/// called, so a crash or timeout mid-call leaves a record pointing at the right server and key id
/// rather than a key on the server HostPanel knows nothing about. The agent's add is idempotent by
/// key id, so a later retry cannot install it twice.
/// </remarks>
public sealed class AddMySshKeyHandler
{
    /// <summary>The rule that says a client may only touch their own services.</summary>
    private readonly IServiceOwnership _ownership;

    /// <summary>Resolves server, provider and account once ownership is settled.</summary>
    private readonly ISshServiceContextResolver _resolver;

    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Stages the activity log line in the same save as the key.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Logger.</summary>
    private readonly ILogger<AddMySshKeyHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own services.</param>
    /// <param name="resolver">Resolves server, provider and account once ownership is settled.</param>
    /// <param name="context">Data context.</param>
    /// <param name="activity">Stages the activity log line in the same save as the key.</param>
    /// <param name="logger">Records a provider failure without putting its detail on the wire.</param>
    public AddMySshKeyHandler(
        IServiceOwnership ownership, ISshServiceContextResolver resolver, IHostpanelDbContext context,
        IActivityLogger activity, ILogger<AddMySshKeyHandler> logger)
    {
        _ownership = ownership;
        _resolver = resolver;
        _context = context;
        _activity = activity;
        _logger = logger;
    }

    /// <summary>Handles <see cref="AddMySshKeyCommand"/>.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The installed key, or one of <c>SERVICE_NOT_FOUND</c>, <c>SSH_ACCESS_UNAVAILABLE</c>,
    /// <c>SSH_KEY_INVALID</c>, <c>SSH_PRIVATE_KEY_REJECTED</c>, <c>SSH_KEY_ALREADY_INSTALLED</c>,
    /// <c>SSH_KEY_LIMIT_REACHED</c>, <c>SSH_SERVER_UNAVAILABLE</c>.
    /// </returns>
    public async Task<Result<SshKeyDto>> HandleAsync(AddMySshKeyCommand cmd, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(cmd.ServiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            // A failed Result always carries its code.
            return Result<SshKeyDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        var context = await _resolver.ResolveAsync(cmd.ServiceId, ct);
        if (context is null)
        {
            return Result<SshKeyDto>.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (context.State is not SshAccessState.Available)
        {
            return Result<SshKeyDto>.FailureCode(HostpanelErrorCodes.SSH_ACCESS_UNAVAILABLE);
        }

        if (!SshPublicKey.TryParse(cmd.PublicKey, out var parsed, out var isPrivate))
        {
            return Result<SshKeyDto>.FailureCode(isPrivate
                ? HostpanelErrorCodes.SSH_PRIVATE_KEY_REJECTED
                : HostpanelErrorCodes.SSH_KEY_INVALID);
        }

        var existing = await _context.SshKeys
            .Where(k => k.ClientServiceId == context.Service.Id && k.SyncState != SshKeySyncState.Revoked)
            .Select(k => k.Fingerprint)
            .ToListAsync(ct);
        // TryParse returned true above, and it sets the parsed key whenever it does.
        if (existing.Contains(parsed!.Fingerprint))
        {
            return Result<SshKeyDto>.FailureCode(HostpanelErrorCodes.SSH_KEY_ALREADY_INSTALLED);
        }

        if (existing.Count >= SshLimits.MaxKeysPerService)
        {
            return Result<SshKeyDto>.FailureCode(HostpanelErrorCodes.SSH_KEY_LIMIT_REACHED);
        }

        // An Available context always carries its account and provider (see the resolver).
        var account = context.Account!;
        var key = SshKey.Create(
            context.Service.Id, account.ServerId, account.Username, cmd.Label, parsed.Canonical, parsed.Fingerprint);
        _context.SshKeys.Add(key);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A concurrent add of the same key won the filtered unique index first.
            return Result<SshKeyDto>.FailureCode(HostpanelErrorCodes.SSH_KEY_ALREADY_INSTALLED);
        }

        try
        {
            await context.Provider!.AddKeyAsync(account, key.KeyId, key.PublicKey, ct);
            key.MarkInstalled();
        }
        catch (SshProviderException ex)
        {
            _logger.LogWarning(ex, "SSH key {KeyId} install on server {ServerId} failed: {Failure}",
                key.KeyId, account.ServerId, ex.Failure);

            if (!ex.IsOutcomeUnknown)
            {
                // The agent answered with a definite refusal: the key is not on the server, so
                // the record would only describe something that never happened.
                _context.SshKeys.Remove(key);
                await _context.SaveChangesAsync(ct);
                return Result<SshKeyDto>.FailureCode(ex.Failure switch
                {
                    SshProviderFailure.DuplicateKey => HostpanelErrorCodes.SSH_KEY_ALREADY_INSTALLED,
                    SshProviderFailure.TooManyKeys => HostpanelErrorCodes.SSH_KEY_LIMIT_REACHED,
                    SshProviderFailure.SshDisabled or SshProviderFailure.AccountSuspended or SshProviderFailure.AccountNotEligible
                        => HostpanelErrorCodes.SSH_ACCESS_UNAVAILABLE,
                    SshProviderFailure.InvalidKey => HostpanelErrorCodes.SSH_KEY_INVALID,
                    _ => HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE,
                });
            }

            key.MarkInstallFailed(ex.Failure);
            await _context.SaveChangesAsync(ct);
            return Result<SshKeyDto>.FailureCode(HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE);
        }

        await _activity.RecordAsync(
            new ActivityLogEntry(
                context.Service.ClientId,
                $"Added SSH key \"{key.Label}\" ({key.Fingerprint}) to service #{context.Service.Id}"),
            ct);
        await _context.SaveChangesAsync(ct);

        return Result<SshKeyDto>.Success(key.ToDto(context, presentOnServer: true));
    }
}
