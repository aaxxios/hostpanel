namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>An invoice an email template refers to by number.</summary>
/// <param name="Id">The invoice id, read as <c>id</c>.</param>
public sealed record InvoiceRefTemplateData(int Id);
