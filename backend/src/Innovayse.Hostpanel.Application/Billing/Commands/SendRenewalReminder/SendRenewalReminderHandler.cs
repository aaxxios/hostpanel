using System.Globalization;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Application.Notifications.Seeding;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalReminder;

/// <summary>
/// Handles <see cref="SendRenewalReminderCommand"/>: for one unpaid renewal invoice, picks the
/// reminder threshold it has reached (if any), mails the client once and records the threshold.
/// Runs in its own message scope, so a failure here never touches another invoice.
/// </summary>
public sealed class SendRenewalReminderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Saves the reminder row and seeds the template.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Reads the reminder thresholds.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Resolves the client's sign-in address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>The client portal's base URL, for the invoice link.</summary>
    private readonly IOptions<ClientPortalOptions> _clientPortal;

    /// <summary>Message bus, for the mail.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<SendRenewalReminderHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="uow">Saves the reminder row and seeds the template.</param>
    /// <param name="settings">Reads the reminder thresholds.</param>
    /// <param name="identity">Resolves the client's sign-in address.</param>
    /// <param name="clientPortal">The client portal's base URL, for the invoice link.</param>
    /// <param name="bus">Message bus, for the mail.</param>
    /// <param name="logger">Structured logger.</param>
    public SendRenewalReminderHandler(
        IHostpanelDbContext context,
        IUnitOfWork uow,
        RenewalSettingsReader settings,
        IIdentityProvider identity,
        IOptions<ClientPortalOptions> clientPortal,
        IMessageBus bus,
        ILogger<SendRenewalReminderHandler> logger)
    {
        _context = context;
        _uow = uow;
        _settings = settings;
        _identity = identity;
        _clientPortal = clientPortal;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Sends and records the reminder the invoice is due, if any.</summary>
    /// <param name="cmd">The invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, including when nothing was due or another run already sent it.</returns>
    public async Task<Result> HandleAsync(SendRenewalReminderCommand cmd, CancellationToken ct)
    {
        var settings = await _settings.ReadAsync(ct);
        if (settings.ReminderDays.Count == 0)
        {
            return Result.Success();
        }

        var invoice = await _context.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.Status == InvoiceStatus.Unpaid
                                      && i.Items.Any(l => l.RenewalPeriodStart != null), ct);
        if (invoice is null)
        {
            return Result.Success();
        }

        var now = DateTimeOffset.UtcNow;
        var daysLeft = (int)Math.Ceiling((invoice.DueDate - now).TotalDays);
        var leadAtCreation = (int)Math.Ceiling((invoice.DueDate - invoice.InvoiceDate).TotalDays);

        // The smallest threshold still ahead of us, among those the invoice was raised early enough
        // to have reached: one raised two days before due never gets a "7 days left" mail, and none
        // goes out on the day invoice-created already did.
        var threshold = settings.ReminderDays.Where(d => d < leadAtCreation && d >= daysLeft).DefaultIfEmpty(-1).Min();
        if (threshold < 0 || daysLeft < 0)
        {
            return Result.Success();
        }

        if (await _context.InvoiceReminders.AnyAsync(r => r.InvoiceId == invoice.Id && r.DaysBefore == threshold, ct))
        {
            return Result.Success();
        }

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == invoice.ClientId, ct);
        var email = client is { NotifyInvoice: true } ? await _context.FindClientEmailAsync(_identity, invoice.ClientId, ct) : null;
        if (email is not null)
        {
            await RenewalTemplatesSeeder.EnsureSeededAsync(_context, _uow, ct);

            // Dictionaries, not nested anonymous objects: Fluid renders the members of a nested
            // anonymous type as blank, so {{ invoice.id }} would be empty.
            var data = new Dictionary<string, object?>
            {
                ["invoice"] = new Dictionary<string, object?>
                {
                    ["id"] = invoice.Id,
                    ["total"] = invoice.Total,
                    ["currency"] = invoice.Currency,
                    ["due_date"] = invoice.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    ["url"] = $"{_clientPortal.Value.BaseUrl.TrimEnd('/')}/client/invoices/{invoice.Id}",
                },
            };

            // Mail first, record second, so a crash between the two repeats the mail on the next run
            // rather than recording a reminder that was never attempted. This is not at-least-once
            // delivery: SendEmailHandler swallows a relay failure (it records it in EmailLogs and
            // answers success), so a reminder the relay refused still spends its threshold here. The
            // failed send is visible in the email log, and the next threshold mails again.
            await _bus.InvokeAsync(new SendEmailCommand(email, RenewalTemplatesSeeder.ReminderSlug, data), ct);
        }

        // Recorded even when nobody was mailed (opted out, no address), so the threshold is spent.
        var row = InvoiceReminder.Create(invoice.Id, threshold, now);
        _context.InvoiceReminders.Add(row);
        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The (InvoiceId, DaysBefore) unique index: a concurrent run recorded it first.
            _context.InvoiceReminders.Entry(row).State = EntityState.Detached;
            if (!await _context.InvoiceReminders.AnyAsync(r => r.InvoiceId == invoice.Id && r.DaysBefore == threshold, ct))
            {
                throw;
            }

            _logger.LogInformation(
                "Reminder for invoice {InvoiceId} at {Threshold} days was already recorded by another run.", invoice.Id, threshold);
        }

        return Result.Success();
    }
}
