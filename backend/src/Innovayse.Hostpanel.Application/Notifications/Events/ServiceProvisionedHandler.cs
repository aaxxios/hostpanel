using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Events;

/// <summary>Handles <see cref="ServiceProvisionedEvent"/> and dispatches a service-ready notification email.</summary>
public sealed class ServiceProvisionedHandler
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
    public ServiceProvisionedHandler(IMessageBus bus, IHostpanelDbContext context, IIdentityProvider identity)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
    }

    /// <summary>Resolves the client email and sends a service-provisioned notification email.</summary>
    /// <param name="evt">The service provisioned domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public async Task HandleAsync(ServiceProvisionedEvent evt, CancellationToken ct)
    {
        var service = await _context.ClientServices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == evt.ServiceId, ct);
        if (service is null)
        {
            return;
        }

        var email = await _context.FindClientEmailAsync(_identity, evt.ClientId, ct);
        if (email is null)
        {
            return;
        }

        var data = new
        {
            service = new
            {
                id = evt.ServiceId,
                provisioningRef = evt.ProvisioningRef,
                domain = service.Domain,
                username = service.Username
            }
        };

        await _bus.InvokeAsync(new SendEmailCommand(email, "service-provisioned", data), ct);
    }
}
