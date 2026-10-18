using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Clients.Events;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Events;

/// <summary>Handles <see cref="ClientCreatedEvent"/> and dispatches a welcome email to the new client.</summary>
public sealed class ClientRegisteredHandler
{
    /// <summary>The Wolverine bus this handler dispatches the notification email through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">The Wolverine bus this handler dispatches the notification email through.</param>
    public ClientRegisteredHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Sends a welcome email when a new client registers.</summary>
    /// <param name="evt">The client created domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public async Task HandleAsync(ClientCreatedEvent evt, CancellationToken ct)
    {
        var data = new { client = new { email = evt.Email } };
        await _bus.InvokeAsync(new SendEmailCommand(evt.Email, "welcome", data), ct);
    }
}
