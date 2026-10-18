namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One billing transaction as the source plugin describes it.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="Date">The date.</param>
/// <param name="Description">The description.</param>
/// <param name="TransactionId">The transaction identifier.</param>
/// <param name="InvoiceId">The invoice identifier.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="AmountIn">The amount in.</param>
/// <param name="AmountOut">The amount out.</param>
/// <param name="Fees">The fees.</param>
/// <param name="Currency">The currency.</param>
internal sealed record TransactionRecord(string ClientEmail, DateTimeOffset Date, string Description, string TransactionId, int? InvoiceId, string PaymentMethod, decimal AmountIn, decimal AmountOut, decimal Fees, string? Currency);
