using Innovayse.Hostpanel.Application.Services.Commands.RetireTerminatedService;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Events;

/// <summary>
/// Settles a terminated service: dispatches <see cref="RetireTerminatedServiceCommand"/>, which closes
/// its open cancellation requests and revokes its SSH keys on the server each was issued on. A non-notification handler,
/// and safe as one: it only triggers that idempotent command, whatever path terminated the service.
/// </summary>
public sealed class ClientServiceTerminatedHandler
{
    /// <summary>Dispatches the command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Records a failure, so the leftovers can be settled by hand.</summary>
    private readonly ILogger<ClientServiceTerminatedHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Dispatches the command.</param>
    /// <param name="logger">Records a failure, so the leftovers can be settled by hand.</param>
    public ClientServiceTerminatedHandler(IMessageBus bus, ILogger<ClientServiceTerminatedHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Settles the terminated service.</summary>
    /// <param name="evt">The termination.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(ClientServiceTerminatedEvent evt, CancellationToken ct)
    {
        try
        {
            var outcome = await _bus.InvokeAsync<Result>(new RetireTerminatedServiceCommand(evt.ServiceId), ct);
            if (outcome.IsFailure)
            {
                _logger.LogWarning(
                    "Settling terminated service {ServiceId} was refused with {ErrorCode}.", evt.ServiceId, outcome.ErrorCode);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Wolverine combines every handler of one message into a single chain, so a throw here
            // would also stop the other handlers of the termination.
            _logger.LogError(ex, "Settling terminated service {ServiceId} failed.", evt.ServiceId);
        }
    }
}
