using Innovayse.Hostpanel.Application.Services.Commands.SetupService;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Services.Commands.SetupMyService;
/// <summary>
/// Sets up and provisions one of the calling client's own pending services, refusing every service
/// that is not theirs.
/// </summary>
/// <remarks>
/// The check lives here rather than at the endpoint, so it travels with the message: nothing can
/// provision through <see cref="SetupMyServiceCommand"/> without it having run. Refusing first
/// also keeps a stranger's id away from <see cref="SetupServiceCommand"/>, whose own refusals
/// distinguish "not found" from "not pending" and would otherwise report the state of somebody
/// else's service. Once ownership is settled the work is unchanged, so this dispatches the shared
/// command rather than duplicating provisioning.
/// </remarks>
public sealed class SetupMyServiceHandler
{
    /// <summary>The rule that says a client may only touch their own services.</summary>
    private readonly IServiceOwnership _ownership;

    /// <summary>Wolverine bus, used to reach the shared use case once ownership is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own services.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared use case once ownership is settled.</param>
    public SetupMyServiceHandler(IServiceOwnership ownership, IMessageBus bus)
    {
        _ownership = ownership;
        _bus = bus;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The command. It names no account: this acts on the caller's own service.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the service is not the caller's, when no such
    /// service exists, or when the caller has no client record, or <c>SERVICE_NOT_PENDING</c>,
    /// <c>NO_ELIGIBLE_SERVER</c> or <c>PROVISIONING_UPSTREAM_FAILED</c> from the shared use case.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result> HandleAsync(SetupMyServiceCommand cmd, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(cmd.ServiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return ownershipOutcome;
        }

        return await _bus.InvokeAsync<Result>(
            new SetupServiceCommand(cmd.ServiceId, cmd.Domain, cmd.Username, cmd.Password), ct);
    }
}
