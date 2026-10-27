using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Seeding;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Events;

/// <summary>
/// Handles <see cref="DomainRenewalFailedEvent"/> by mailing staff: a paid or due domain renewal
/// that the billing engine could not complete on its own (registrar refusal, already renewed, gone).
/// </summary>
public sealed class DomainRenewalFailedAdminHandler
{
    /// <summary>Sends the mail.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Where staff mail goes.</summary>
    private readonly IOptions<NotificationOptions> _settings;

    /// <summary>For seeding the template.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>For seeding the template.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Names the missing setting when no staff address is configured.</summary>
    private readonly ILogger<DomainRenewalFailedAdminHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Sends the mail.</param>
    /// <param name="settings">Where staff mail goes.</param>
    /// <param name="context">For seeding the template.</param>
    /// <param name="uow">For seeding the template.</param>
    /// <param name="logger">Names the missing setting when no staff address is configured.</param>
    public DomainRenewalFailedAdminHandler(
        IMessageBus bus,
        IOptions<NotificationOptions> settings,
        IHostpanelDbContext context,
        IUnitOfWork uow,
        ILogger<DomainRenewalFailedAdminHandler> logger)
    {
        _bus = bus;
        _settings = settings;
        _context = context;
        _uow = uow;
        _logger = logger;
    }

    /// <summary>Mails staff.</summary>
    /// <param name="evt">The failure.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(DomainRenewalFailedEvent evt, CancellationToken ct)
    {
        // A deliberate exception to NotificationOptions' "no fallback" rule: AdminEmail is set on
        // no tier today, and this alert is about money already taken, so it must reach some
        // mailbox a person reads rather than nobody.
        var to = !string.IsNullOrWhiteSpace(_settings.Value.AdminEmail)
            ? _settings.Value.AdminEmail
            : _settings.Value.ContactEmail;
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogError(
                "Domain renewal alert for {DomainName} (invoice {InvoiceId}) not mailed: set {Setting} (or Notifications:ContactEmail). Reason: {Reason}",
                evt.DomainName, evt.InvoiceId, "Notifications:AdminEmail", evt.Reason);
            return;
        }

        await RenewalTemplatesSeeder.EnsureSeededAsync(_context, _uow, ct);
        // Dictionaries, not nested anonymous objects: Fluid resolves a dictionary's keys at every
        // depth, but the members of a nested anonymous type only when that type is registered, so
        // {{ domain.name }} over `new { domain = new { name } }` renders empty.
        var data = new Dictionary<string, object?>
        {
            ["domain"] = new Dictionary<string, object?> { ["name"] = evt.DomainName },
            ["client"] = new Dictionary<string, object?> { ["id"] = evt.ClientId },
            ["invoice"] = new Dictionary<string, object?> { ["id"] = evt.InvoiceId },
            ["reason"] = evt.Reason,
        };
        await _bus.InvokeAsync(new SendEmailCommand(to, RenewalTemplatesSeeder.DomainFailedAdminSlug, data), ct);
    }
}
