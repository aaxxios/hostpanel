using Innovayse.Hostpanel.Application.Reports.Common.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One line in a client statement.</summary>
/// <param name="Type">What the line records.</param>
/// <param name="Date">The day of the line (UTC), <c>yyyy-MM-dd</c>.</param>
/// <param name="InvoiceId">The invoice the line records; null for a transaction line.</param>
/// <param name="Description">The transaction description; null for an invoice or history payment line, which clients render from <paramref name="InvoiceId"/>.</param>
/// <param name="Currency">The ISO 4217 code every amount of the line is in.</param>
/// <param name="Debit">The amount billed or paid out.</param>
/// <param name="Credit">The amount paid in.</param>
/// <param name="Balance">The running balance of the line's currency after the line; positive means the client owes.</param>
public record ClientStatementLineDto(
    StatementLineType Type,
    string Date,
    int? InvoiceId,
    string? Description,
    string Currency,
    decimal Debit,
    decimal Credit,
    decimal Balance);
