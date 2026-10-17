using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Support.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Support.Events;

/// <summary>
/// Handles <see cref="TicketClosedEvent"/> raised when a support ticket is closed.
/// Notifies the client so they know the ticket is resolved and can leave feedback.
/// </summary>
public sealed class TicketClosedHandler
{
    /// <summary>Wolverine bus, used to dispatch the notification email.</summary>
    private readonly IMessageBus _bus;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the identity account behind a client's subject.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus, used to dispatch the notification email.</param>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Resolves the identity account behind a client's subject.</param>
    public TicketClosedHandler(IMessageBus bus, IHostpanelDbContext context, IIdentityProvider identity)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
    }

    /// <summary>
    /// Resolves the client's email and sends a ticket-closed notification email.
    /// </summary>
    /// <param name="evt">The ticket closed domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(TicketClosedEvent evt, CancellationToken ct)
    {
        var userId = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Id == evt.ClientId)
            .Select(c => c.UserId)
            .FirstOrDefaultAsync(ct);
        if (userId is null)
        {
            return;
        }

        var user = await _identity.FindBySubjectAsync(userId, ct);
        if (user is null)
        {
            return;
        }

        var subject = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == evt.TicketId)
            .Select(t => t.Subject)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var data = new { ticket = new { id = evt.TicketId, subject } };

        await _bus.InvokeAsync(new SendEmailCommand(user.Email, "ticket-closed", data), ct);
    }
}
