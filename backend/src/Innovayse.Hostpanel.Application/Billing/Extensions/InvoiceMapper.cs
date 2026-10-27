using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Maps a domain <see cref="Invoice"/> to <see cref="InvoiceDto"/>. Every read of a full invoice
/// goes through this one mapping — <c>GetInvoiceHandler</c> (the admin read), <c>GetMyInvoicesHandler</c>
/// and <c>GetMyServiceInvoicesHandler</c> (the client-facing reads) — so a field added to the DTO
/// is added once rather than in three places that quietly drift apart.
/// </summary>
public static class InvoiceMapper
{
    /// <summary>Maps a domain invoice to a full DTO.</summary>
    /// <param name="invoice">The domain invoice.</param>
    /// <param name="clientName">Display name of the owning client. <c>null</c> when the caller has no name to give.</param>
    /// <returns>The mapped DTO.</returns>
    public static InvoiceDto MapToDto(Invoice invoice, string? clientName = null) =>
        new(
            invoice.Id,
            invoice.ClientId,
            invoice.Status,
            invoice.DueDate,
            invoice.CreatedAt,
            invoice.PaidAt,
            invoice.Total,
            invoice.SubTotal,
            invoice.Tax,
            invoice.TaxRate,
            invoice.Credit,
            invoice.AmountDue,
            invoice.GatewayTransactionId,
            invoice.Notes,
            invoice.InvoiceDate,
            invoice.PaymentMethod,
            clientName,
            invoice.Currency,
            invoice.Items.Select(i => new InvoiceItemDto(i.Id, i.Description, i.UnitPrice, i.Quantity, i.Amount))
                         .ToList(),
            invoice.Transactions.Select(t => new InvoiceTransactionDto(
                t.Id, t.Date, t.Gateway, t.TransactionId, t.Amount, t.Fees, t.Type, t.Notes))
                         .ToList());
}
