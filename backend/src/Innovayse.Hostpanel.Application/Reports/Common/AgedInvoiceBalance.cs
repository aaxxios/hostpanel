using Innovayse.Hostpanel.Application.Reports.Common.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// What an open invoice still owes and how far past due it is, as the two aging reports read it.
/// A class with init-only members rather than a positional record, so a query can keep filtering,
/// grouping and ordering on its members after the projection and the database still runs all of it.
/// </summary>
public sealed class AgedInvoiceBalance
{
    /// <summary>Gets the invoice identifier.</summary>
    public required int InvoiceId { get; init; }

    /// <summary>Gets the client the invoice bills.</summary>
    public required int ClientId { get; init; }

    /// <summary>Gets the ISO 4217 code the invoice bills in.</summary>
    public required string Currency { get; init; }

    /// <summary>Gets the payment due date (UTC).</summary>
    public required DateTimeOffset DueDate { get; init; }

    /// <summary>Gets what is still owed: the total less the payments recorded on the invoice.</summary>
    public required decimal AmountDue { get; init; }

    /// <summary>Gets the aging period the due date falls in.</summary>
    public required AgingBucket Bucket { get; init; }
}
