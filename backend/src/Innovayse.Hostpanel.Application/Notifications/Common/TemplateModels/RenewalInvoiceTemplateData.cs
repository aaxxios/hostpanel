namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A renewal invoice as the reminder email reads it.</summary>
/// <param name="Id">The invoice id, read as <c>id</c>.</param>
/// <param name="Total">The amount due, read as <c>total</c>.</param>
/// <param name="Currency">The invoice currency code, read as <c>currency</c>.</param>
/// <param name="DueDate">The due date as <c>yyyy-MM-dd</c>, read as <c>due_date</c>.</param>
/// <param name="Url">The absolute link to the invoice in the client area, read as <c>url</c>.</param>
public sealed record RenewalInvoiceTemplateData(int Id, decimal Total, string Currency, string DueDate, string Url);
