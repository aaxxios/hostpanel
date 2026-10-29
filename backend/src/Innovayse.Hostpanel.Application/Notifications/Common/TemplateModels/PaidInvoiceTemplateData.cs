namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A paid invoice as the payment email reads it.</summary>
/// <param name="Id">The invoice id, read as <c>id</c>.</param>
/// <param name="Total">The amount of this payment, read as <c>total</c>.</param>
/// <param name="TransactionId">The gateway transaction reference, read as <c>transaction_id</c>.</param>
public sealed record PaidInvoiceTemplateData(int Id, decimal Total, string TransactionId);
