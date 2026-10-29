namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the staff alert about a gateway payment that stays pending for an unusual time.</summary>
/// <param name="Invoice">The invoice, read as <c>{{ invoice.id }}</c>.</param>
/// <param name="Client">The owning client, read as <c>{{ client.id }}</c>.</param>
/// <param name="Module">The payment plugin id, read as <c>{{ module }}</c>.</param>
/// <param name="PendingHours">Whole hours the payment has been pending, read as <c>{{ pending_hours }}</c>.</param>
public sealed record GatewayPaymentStalledAdminTemplateModel(
    InvoiceRefTemplateData Invoice, ClientRefTemplateData Client, string Module, int PendingHours) : EmailTemplateModel;
