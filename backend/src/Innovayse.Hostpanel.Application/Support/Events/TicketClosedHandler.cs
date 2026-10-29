using Innovayse.Hostpanel.Application.Clients.Common.Enums;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;
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

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus, used to dispatch the notification email.</param>
    /// <param name="context">The module's data context.</param>
    public TicketClosedHandler(IMessageBus bus, IHostpanelDbContext context)
    {
        _bus = bus;
        _context = context;
    }

    /// <summary>
    /// Resolves the client's email and sends a ticket-closed notification email.
    /// </summary>
    /// <param name="evt">The ticket closed domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(TicketClosedEvent evt, CancellationToken ct)
    {
        var client = await _bus.FindClientBriefAsync(evt.ClientId, withEmail: true, ct);
        if (client is not { Email: { } email })
        {
            return;
        }

        var subject = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == evt.TicketId)
            .Select(t => t.Subject)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var data = new TicketClosedTemplateModel(new TicketTemplateData(evt.TicketId, subject));

        await _bus.InvokeAsync(new SendEmailCommand(email, client.Language, "ticket-closed", data, new ContactCopy(evt.ClientId, ContactNotification.Support)), ct);
    }
}
