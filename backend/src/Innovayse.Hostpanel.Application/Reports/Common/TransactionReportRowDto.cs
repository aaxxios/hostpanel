namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row of the Transactions report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="Currency">The currency.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="Date">The date.</param>
/// <param name="Description">The description.</param>
/// <param name="InvoiceId">The invoice identifier.</param>
/// <param name="TransactionId">The transaction identifier.</param>
/// <param name="AmountIn">The amount in.</param>
/// <param name="Fees">The fees.</param>
/// <param name="AmountOut">The amount out.</param>
public record TransactionReportRowDto(
    int Id,
    int ClientId,
    string ClientName,
    string Currency,
    string PaymentMethod,
    string Date,
    string Description,
    int? InvoiceId,
    string TransactionId,
    decimal AmountIn,
    decimal Fees,
    decimal AmountOut);
