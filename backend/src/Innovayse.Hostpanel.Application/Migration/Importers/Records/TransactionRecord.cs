namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One billing transaction as the source plugin describes it.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="ExternalId">The transaction's own row id in the source; identifies the record in the migration log.</param>
/// <param name="Date">When the payment or refund happened; null when the source holds no date, and the record is then skipped.</param>
/// <param name="Description">The source's text describing the transaction.</param>
/// <param name="TransactionId">The gateway transaction reference, or the source row id when the source holds none; the duplicate check per client.</param>
/// <param name="InvoiceId">The source invoice number the transaction pays, matched to an imported invoice by its external id; null when unlinked.</param>
/// <param name="PaymentMethod">The source's gateway name for how it is paid; mapped to a local gateway where one matches.</param>
/// <param name="AmountIn">Amount received from the client.</param>
/// <param name="AmountOut">Amount paid back to the client (refunds).</param>
/// <param name="Fees">Gateway fees charged on the transaction.</param>
/// <param name="Currency">ISO 4217 code of the currency the amounts are in; null when the source holds none.</param>
internal sealed record TransactionRecord(
    string ClientEmail, string ExternalId, DateTimeOffset? Date, string Description, string TransactionId,
    string? InvoiceId, string PaymentMethod, decimal AmountIn, decimal AmountOut, decimal Fees, string? Currency);
