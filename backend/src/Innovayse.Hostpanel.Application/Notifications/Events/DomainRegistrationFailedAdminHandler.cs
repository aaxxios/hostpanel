using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Events;

/// <summary>
/// Handles <see cref="DomainRegistrationFailedEvent"/> and alerts the platform admin
/// by email so the team is aware of failed registrations that required automatic refunds.
/// </summary>
public sealed class DomainRegistrationFailedAdminHandler
{
    /// <summary>The Wolverine bus this handler dispatches the notification email through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>The configured platform admin address the alert is sent to.</summary>
    private readonly IOptions<NotificationOptions> _settings;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">The Wolverine bus this handler dispatches the notification email through.</param>
    /// <param name="settings">The configured platform admin address.</param>
    public DomainRegistrationFailedAdminHandler(IMessageBus bus, IOptions<NotificationOptions> settings)
    {
        _bus = bus;
        _settings = settings;
    }

    /// <summary>Dispatches a domain-registration-failed-admin notification email to the configured admin address.</summary>
    /// <param name="evt">The domain registration failed event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public async Task HandleAsync(DomainRegistrationFailedEvent evt, CancellationToken ct)
    {
        var data = new DomainRegistrationFailedAdminTemplateModel(
            new DomainNameTemplateData(evt.DomainName),
            new ClientRefTemplateData(evt.ClientId),
            new InvoiceRefTemplateData(evt.InvoiceId),
            evt.Reason);

        await _bus.InvokeAsync(
            new SendEmailCommand(_settings.Value.AdminEmail, null, "domain-registration-failed-admin", data), ct);
    }
}
