namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>The dates of an invoice.</summary>
/// <param name="InvoiceDate">Invoice issue date (UTC).</param>
/// <param name="DueDate">Payment due date (UTC).</param>
/// <param name="CreatedAt">Creation timestamp (UTC).</param>
/// <param name="PaidAt">Payment timestamp (UTC); null when never paid, kept after a refund.</param>
public sealed record InvoiceDatesDto(
    DateTimeOffset InvoiceDate,
    DateTimeOffset DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt);
