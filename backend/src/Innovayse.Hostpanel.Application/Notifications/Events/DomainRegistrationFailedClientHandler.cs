using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Events;

/// <summary>
/// Handles <see cref="DomainRegistrationFailedEvent"/> and notifies the affected client
/// by email that their domain registration failed and a refund has been issued.
/// </summary>
public sealed class DomainRegistrationFailedClientHandler
{
    /// <summary>The Wolverine bus this handler dispatches the notification email through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the client's sign-in address for the notification email.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">The Wolverine bus this handler dispatches the notification email through.</param>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Resolves the client's sign-in address.</param>
    public DomainRegistrationFailedClientHandler(IMessageBus bus, IHostpanelDbContext context, IIdentityProvider identity)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
    }

    /// <summary>Resolves the client email and dispatches a domain-registration-failed notification email.</summary>
    /// <param name="evt">The domain registration failed event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public async Task HandleAsync(DomainRegistrationFailedEvent evt, CancellationToken ct)
    {
        var email = await _context.FindClientEmailAsync(_identity, evt.ClientId, ct);
        if (email is null)
        {
            return;
        }

        var data = new
        {
            domain = new { name = evt.DomainName },
            invoice = new { id = evt.InvoiceId },
        };

        await _bus.InvokeAsync(
            new SendEmailCommand(email, "domain-registration-failed", data), ct);
    }
}
