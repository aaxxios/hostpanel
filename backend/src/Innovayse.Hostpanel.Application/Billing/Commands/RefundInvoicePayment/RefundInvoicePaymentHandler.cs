using Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment.Enums;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Constants;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment;

/// <summary>
/// Records a refund against a paid invoice, creates a client-level transaction ledger entry,
/// and optionally adds the refund amount to the client's credit balance (for CreditBalance refunds).
/// For invoices paid through a hosted-gateway payment plugin, the gateway refund is executed
/// first and the refund is recorded only on success.
/// </summary>
public sealed class RefundInvoicePaymentHandler
{
    /// <summary>Gateway label recorded on the ledger for a refund credited to the client's balance. Persisted value, unchanged.</summary>
    private const string CreditBalanceGateway = PaymentMethodNames.CreditBalance;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the hosted-gateway payment plugin that processed the original payment.</summary>
    private readonly IPaymentPluginResolver _pluginResolver;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="pluginResolver">Resolves the hosted-gateway plugin for the invoice's gateway.</param>
    public RefundInvoicePaymentHandler(IHostpanelDbContext context, IPaymentPluginResolver pluginResolver)
    {
        _context = context;
        _pluginResolver = pluginResolver;
    }

    /// <summary>Handles <see cref="RefundInvoicePaymentCommand"/>.</summary>
    /// <param name="cmd">The refund command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice; <c>CLIENT_NOT_FOUND</c>
    /// when a CreditBalance refund cannot credit an owning client that no longer exists; or
    /// <c>PAYMENT_PLUGIN_NOT_CONFIGURED</c> when the invoice was paid through a hosted-gateway
    /// module with no configured plugin.
    /// </returns>
    public async Task<Result> HandleAsync(RefundInvoicePaymentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        // Determine the actual refund amount (0 means full refund of the original transaction)
        var refundAmount = cmd.Amount > 0 ? cmd.Amount : invoice.Total;

        // Determine the transaction reference based on refund type
        string txnRef;
        if (cmd.RefundType == nameof(RefundType.Manual))
        {
            txnRef = cmd.RefundTransactionId ?? $"manual-refund-{cmd.InvoiceId}-{DateTimeOffset.UtcNow.Ticks}";
        }
        else if (cmd.RefundType == nameof(RefundType.CreditBalance))
        {
            txnRef = $"credit-refund-{cmd.InvoiceId}-{DateTimeOffset.UtcNow.Ticks}";
        }
        else
        {
            var gatewayResult = await ExecuteGatewayRefundAsync(invoice, refundAmount, ct);
            if (gatewayResult.IsFailure)
            {
                return Result.FailureCode(gatewayResult.ErrorCode!);
            }

            txnRef = gatewayResult.Value!;
        }

        var gateway = cmd.RefundType == nameof(RefundType.CreditBalance) ? CreditBalanceGateway : cmd.Gateway;
        var addToCredit = cmd.RefundType == nameof(RefundType.CreditBalance);

        invoice.AddRefund(DateTimeOffset.UtcNow, gateway, txnRef, refundAmount, 0m, cmd.Notes);

        var clientTx = Transaction.Create(
            invoice.ClientId,
            DateTimeOffset.UtcNow,
            $"Invoice #{cmd.InvoiceId} Refund",
            txnRef,
            cmd.InvoiceId,
            gateway,
            0m,
            refundAmount,
            0m,
            addedToCredit: addToCredit,
            invoice.Currency);
        _context.Transactions.Add(clientTx);

        if (addToCredit)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == invoice.ClientId, ct);
            if (client is null)
            {
                return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
            }

            client.AddCredit(refundAmount);
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    /// <summary>
    /// Executes a real gateway refund when the invoice was paid through a hosted-gateway
    /// plugin; falls back to a bookkeeping-only reference otherwise (legacy behavior).
    /// </summary>
    /// <param name="invoice">The invoice being refunded.</param>
    /// <param name="refundAmount">The refund amount in major units.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the transaction reference to record, or <c>PAYMENT_PLUGIN_NOT_CONFIGURED</c>
    /// when the invoice's gateway module has no configured plugin.
    /// </returns>
    private async Task<Result<string>> ExecuteGatewayRefundAsync(
        Invoice invoice, decimal refundAmount, CancellationToken ct)
    {
        if (invoice.GatewayModule is null || invoice.GatewayOrderId is null)
        {
            return Result<string>.Success($"gateway-refund-{invoice.Id}-{DateTimeOffset.UtcNow.Ticks}");
        }

        var plugin = await _pluginResolver.ResolveAsync(invoice.GatewayModule, ct);
        if (plugin is null)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.PAYMENT_PLUGIN_NOT_CONFIGURED);
        }

        // Gateway call first: if it fails, nothing is recorded and books stay consistent.
        var amountMinor = CurrencyCodes.ToMinorUnits(refundAmount);
        var reference = await plugin.RefundAsync(invoice.GatewayOrderId, amountMinor, ct);
        return Result<string>.Success(reference);
    }
}
