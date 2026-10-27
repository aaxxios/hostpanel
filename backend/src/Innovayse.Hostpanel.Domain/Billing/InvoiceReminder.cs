using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Billing;

/// <summary>
/// A renewal reminder already sent for an invoice, so each configured day is mailed once.
/// Stored in the <c>InvoiceReminders</c> table.
/// </summary>
public sealed class InvoiceReminder : BaseEntity
{
    /// <summary>Gets the invoice the reminder was about.</summary>
    public int InvoiceId { get; private set; }

    /// <summary>Gets the configured threshold (days before due) this reminder answered.</summary>
    public int DaysBefore { get; private set; }

    /// <summary>Gets when it was sent (UTC).</summary>
    public DateTimeOffset SentAt { get; private set; }

    /// <summary>EF Core parameterless constructor — do not call directly.</summary>
    private InvoiceReminder() { }

    /// <summary>Records a sent reminder.</summary>
    /// <param name="invoiceId">The invoice.</param>
    /// <param name="daysBefore">The threshold it answered.</param>
    /// <param name="sentAt">When it was sent.</param>
    /// <returns>The new record.</returns>
    public static InvoiceReminder Create(int invoiceId, int daysBefore, DateTimeOffset sentAt) =>
        new() { InvoiceId = invoiceId, DaysBefore = daysBefore, SentAt = sentAt };
}
