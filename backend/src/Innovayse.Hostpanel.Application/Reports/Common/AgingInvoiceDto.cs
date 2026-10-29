using Innovayse.Hostpanel.Application.Reports.Common.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row of the Aging Invoices report.</summary>
/// <param name="InvoiceId">The invoice identifier.</param>
/// <param name="Client">The client's full name.</param>
/// <param name="Currency">The ISO 4217 code the invoice bills in, which <paramref name="AmountDue"/> is in.</param>
/// <param name="AmountDue">What the invoice still owes: its total less the payments recorded on it.</param>
/// <param name="DueDate">The UTC due date, <c>yyyy-MM-dd</c>.</param>
/// <param name="DaysOutstanding">Whole UTC days past the due date; zero when not yet due.</param>
/// <param name="Bucket">The aging period the invoice falls in.</param>
public sealed record AgingInvoiceDto(
    int InvoiceId,
    string Client,
    string Currency,
    decimal AmountDue,
    string DueDate,
    int DaysOutstanding,
    AgingBucket Bucket);
