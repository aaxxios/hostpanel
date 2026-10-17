using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateInvoice;

/// <summary>Creates a new invoice with the provided line items and persists it.</summary>
public sealed class CreateInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Current request context, providing the acting admin's identity and IP for the audit trail.</summary>
    private readonly ICurrentUserService _ctx;

    /// <summary>Resolves the currency the invoiced client is billed in, stamped on the invoice.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="ctx">Current request context providing admin identity and IP.</param>
    /// <param name="payerCurrency">The currency the invoiced client is billed in, stamped on the invoice.</param>
    public CreateInvoiceHandler(
        IHostpanelDbContext context, ICurrentUserService ctx, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _ctx = ctx;
        _payerCurrency = payerCurrency;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create invoice command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the newly created invoice ID. This use case refuses nothing itself.</returns>
    public async Task<Result<int>> HandleAsync(CreateInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var currency = (await _payerCurrency.ForClientAsync(cmd.ClientId, ct)).Code;
        var invoice = Invoice.Create(cmd.ClientId, cmd.DueDate, currency, cmd.IsDraft);

        foreach (var item in cmd.Items)
        {
            invoice.AddItem(item.Description, item.UnitPrice, item.Quantity);
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(ct);

        _context.ActivityLogs.Add(ActivityLog.Create(
            cmd.ClientId,
            $"Created Manual Invoice - Invoice ID: {invoice.Id}",
            _ctx.UserId, _ctx.UserName, _ctx.Email, _ctx.IpAddress));
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(invoice.Id);
    }
}
