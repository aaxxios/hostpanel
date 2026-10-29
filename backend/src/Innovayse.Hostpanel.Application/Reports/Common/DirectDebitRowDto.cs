namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row in the Direct Debit Processing report.</summary>
/// <param name="InvoiceId">The invoice identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="InvoiceDate">The invoice date.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="Subtotal">The subtotal.</param>
/// <param name="Tax">The tax.</param>
/// <param name="Credit">The credit.</param>
/// <param name="Total">The total.</param>
/// <param name="BankName">The bank name.</param>
/// <param name="BankAccountType">The bank account type.</param>
/// <param name="BankCode">The bank code.</param>
/// <param name="BankAccountNumber">The bank account number.</param>
public record DirectDebitRowDto(
    int InvoiceId,
    string ClientName,
    string InvoiceDate,
    string DueDate,
    decimal Subtotal,
    decimal Tax,
    decimal Credit,
    decimal Total,
    string? BankName,
    string? BankAccountType,
    string? BankCode,
    string? BankAccountNumber);
