using Innovayse.Hostpanel.Application.Reports.Common.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One unsorted entry of a client statement, before its running balance is known.</summary>
/// <param name="Date">When the entry happened.</param>
/// <param name="Type">What the entry records.</param>
/// <param name="InvoiceId">The invoice the entry records; null for a transaction.</param>
/// <param name="Description">The transaction description; null for an invoice or a history payment.</param>
/// <param name="Currency">The ISO 4217 code the amounts are in.</param>
/// <param name="Debit">The amount billed or paid out.</param>
/// <param name="Credit">The amount paid in.</param>
public sealed record ClientStatementSource(
    DateTimeOffset Date,
    StatementLineType Type,
    int? InvoiceId,
    string? Description,
    string Currency,
    decimal Debit,
    decimal Credit);
