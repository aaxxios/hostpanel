using Innovayse.Hostpanel.Application.Services.Commands.RestoreServiceSshKeys;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Events;

/// <summary>
/// When a service is unsuspended, dispatches <see cref="RestoreServiceSshKeysCommand"/>, which puts
/// back the SSH keys taken off at suspension. The provisioning handler sends the same command again
/// once the panel unlocked the hosting account. A non-notification handler, and safe as one: it only
/// triggers that idempotent command.
/// </summary>
public sealed class RestoreSshKeysOnServiceUnsuspendedHandler
{
    /// <summary>Dispatches the command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Records a failure, so staff can settle the keys by hand.</summary>
    private readonly ILogger<RestoreSshKeysOnServiceUnsuspendedHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Dispatches the command.</param>
    /// <param name="logger">Records a failure, so staff can settle the keys by hand.</param>
    public RestoreSshKeysOnServiceUnsuspendedHandler(IMessageBus bus, ILogger<RestoreSshKeysOnServiceUnsuspendedHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Handles <see cref="ClientServiceUnsuspendedEvent"/>.</summary>
    /// <param name="evt">The event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the command ran.</returns>
    public async Task HandleAsync(ClientServiceUnsuspendedEvent evt, CancellationToken ct)
    {
        try
        {
            var outcome = await _bus.InvokeAsync<Result>(new RestoreServiceSshKeysCommand(evt.ServiceId), ct);
            if (outcome.IsFailure)
            {
                _logger.LogWarning(
                    "Restoring the SSH keys of service {ServiceId} was refused with {ErrorCode}.", evt.ServiceId, outcome.ErrorCode);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Wolverine combines every handler of one message into a single chain, so a throw here
            // would also stop the other handlers of the same event.
            _logger.LogError(ex, "Restoring the SSH keys of service {ServiceId} failed.", evt.ServiceId);
        }
    }
}
