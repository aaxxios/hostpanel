using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMySshAccess;

/// <summary>
/// Answers whether the caller can manage SSH keys on a service, and where to connect. Asks the
/// server only once every local check passed; a server that does not answer is reported as
/// <see cref="SshAccessState.Unreachable"/>, not as an error.
/// </summary>
public sealed class GetMySshAccessHandler
{
    /// <summary>The rule that says a client may only touch their own services.</summary>
    private readonly IServiceOwnership _ownership;

    /// <summary>Resolves server, provider and account once ownership is settled.</summary>
    private readonly ISshServiceContextResolver _resolver;

    /// <summary>Logger.</summary>
    private readonly ILogger<GetMySshAccessHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own services.</param>
    /// <param name="resolver">Resolves server, provider and account once ownership is settled.</param>
    /// <param name="logger">Logger.</param>
    public GetMySshAccessHandler(
        IServiceOwnership ownership, ISshServiceContextResolver resolver, ILogger<GetMySshAccessHandler> logger)
    {
        _ownership = ownership;
        _resolver = resolver;
        _logger = logger;
    }

    /// <summary>Handles <see cref="GetMySshAccessQuery"/>.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The access summary, or <c>SERVICE_NOT_FOUND</c> when the service is not the caller's.</returns>
    public async Task<Result<SshAccessDto>> HandleAsync(GetMySshAccessQuery query, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.ServiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<SshAccessDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        var context = await _resolver.ResolveAsync(query.ServiceId, ct);
        if (context is null)
        {
            return Result<SshAccessDto>.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        var state = context.State;
        if (state is SshAccessState.Available)
        {
            try
            {
                var status = await context.Provider!.GetAccessStatusAsync(context.Account!, ct);
                if (!status.ShellEnabled)
                {
                    state = SshAccessState.ShellDisabled;
                }
            }
            catch (SshProviderException ex)
            {
                _logger.LogWarning(ex, "SSH status for service {ServiceId} on server {ServerId} failed: {Code}",
                    context.Service.Id, context.Server!.Id, ex.Code);

                // The agent answered that this account cannot have keys: a fact about the account,
                // not an outage, so the customer is sent to support rather than told to retry.
                state = ex.Code is "account_not_found" or "account_not_eligible" or "invalid_account"
                    ? SshAccessState.ShellDisabled
                    : SshAccessState.Unreachable;
            }
        }

        var settings = context.Settings;
        var username = settings is null ? null : context.Account?.Username;
        string? command = null;
        if (state is SshAccessState.Available && settings is not null && username is not null)
        {
            command = settings.SshPort == 22
                ? $"ssh {username}@{settings.SshHostname}"
                : $"ssh -p {settings.SshPort} {username}@{settings.SshHostname}";
        }

        return Result<SshAccessDto>.Success(new SshAccessDto(
            state, settings?.SshHostname, settings?.SshPort, username, command, SshLimits.MaxKeysPerService));
    }
}
