using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Support.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Support.Events;

/// <summary>
/// Handles <see cref="TicketRepliedEvent"/> raised when a reply is added to a ticket.
/// A staff reply notifies the client; a client reply notifies the assigned department.
/// </summary>
public sealed class TicketRepliedHandler
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
    public TicketRepliedHandler(IMessageBus bus, IHostpanelDbContext context, IIdentityProvider identity)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
    }

    /// <summary>
    /// Resolves the recipient (client or department, depending on the reply's author) and sends a
    /// ticket-replied notification email.
    /// </summary>
    /// <param name="evt">The ticket replied domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(TicketRepliedEvent evt, CancellationToken ct)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == evt.TicketId)
            .Select(t => new { t.Subject, t.ClientId, t.DepartmentId })
            .FirstOrDefaultAsync(ct);
        if (ticket is null)
        {
            return;
        }

        var data = new { ticket = new { id = evt.TicketId, ticket.Subject } };

        if (evt.IsStaffReply)
        {
            var userId = await _context.Clients
                .AsNoTracking()
                .Where(c => c.Id == ticket.ClientId)
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

            await _bus.InvokeAsync(new SendEmailCommand(user.Email, "ticket-replied", data), ct);
        }
        else if (ticket.DepartmentId is int departmentId)
        {
            var departmentEmail = await _context.Departments
                .AsNoTracking()
                .Where(d => d.Id == departmentId)
                .Select(d => d.Email)
                .FirstOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(departmentEmail))
            {
                return;
            }

            await _bus.InvokeAsync(new SendEmailCommand(departmentEmail, "ticket-replied", data), ct);
        }
    }
}
