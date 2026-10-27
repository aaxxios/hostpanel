namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalReminder;

/// <summary>
/// Sends the reminder one unpaid renewal invoice is due today, if any, and records it. Idempotent:
/// the <c>InvoiceReminders</c> row for the threshold decides whether anything is sent.
/// </summary>
/// <param name="InvoiceId">The invoice.</param>
public record SendRenewalReminderCommand(int InvoiceId);
