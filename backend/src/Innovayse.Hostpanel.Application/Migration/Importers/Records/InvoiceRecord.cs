namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One invoice as the source plugin describes it, with its line items and totals.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="ExternalId">The invoice number in the source; together with the client it is the duplicate check.</param>
/// <param name="Total">Invoice total in the source, in the client's currency (sub-total plus tax less credit).</param>
/// <param name="Tax">Tax the source charged on the invoice, all tax levels together.</param>
/// <param name="TaxRate">Tax percentage the source applied, all tax levels together.</param>
/// <param name="Credit">Account credit the source applied to the invoice.</param>
/// <param name="Status">The source's status word for the record, mapped to a local status by the importer's source-value table.</param>
/// <param name="Date">Issue date of the invoice; null when the source holds no date, and the record is then skipped.</param>
/// <param name="DueDate">Date the invoice falls due; null when the source holds no date, and the record is then skipped.</param>
/// <param name="PaidAt">When the source recorded the invoice as paid; null when it holds no date.</param>
/// <param name="PaymentMethod">The source's gateway name for how it is paid; mapped to a local gateway where one matches.</param>
/// <param name="Items">The line items, in source order.</param>
internal sealed record InvoiceRecord(
    string ClientEmail, string? ExternalId, decimal Total, decimal Tax, decimal TaxRate, decimal Credit, string Status,
    DateTimeOffset? Date, DateTimeOffset? DueDate, DateTimeOffset? PaidAt,
    string? PaymentMethod,
    List<InvoiceItemRecord> Items);
