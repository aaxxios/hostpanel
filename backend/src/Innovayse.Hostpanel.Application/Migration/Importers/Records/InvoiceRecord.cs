namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One invoice as the source plugin describes it, with its line items.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="ExternalId">The external identifier.</param>
/// <param name="Total">The total.</param>
/// <param name="Status">The status.</param>
/// <param name="Date">The date.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="Items">The items entries.</param>
internal sealed record InvoiceRecord(
    string ClientEmail, string? ExternalId, decimal Total, string Status,
    DateTimeOffset Date, DateTimeOffset DueDate,
    string? PaymentMethod,
    List<InvoiceItemRecord> Items);
