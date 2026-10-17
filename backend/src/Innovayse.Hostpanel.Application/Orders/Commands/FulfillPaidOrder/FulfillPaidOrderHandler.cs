using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Constants;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.RegisterDomain;
using Innovayse.Hostpanel.Application.Domains.Commands.RenewDomain;
using Innovayse.Hostpanel.Application.Domains.Commands.TransferDomain;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Application.Services.Commands.OrderService;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Orders.Commands.FulfillPaidOrder;

/// <summary>
/// Handles <see cref="FulfillPaidOrderCommand"/>: accepts the paid order, dispatches
/// domain registration/transfer or service creation per line item, auto-links a domain
/// to a hosting service bought in the same order, and issues an automatic refund via
/// the invoice's payment gateway when the registrar immediately rejects a domain.
/// </summary>
public sealed class FulfillPaidOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Card payment gateway used when the invoice was paid by card.</summary>
    private readonly ICardPaymentGateway _cardGateway;

    /// <summary>Resolver for hosted-gateway payment plugins.</summary>
    private readonly IPaymentPluginResolver _pluginResolver;

    /// <summary>Wolverine message bus.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<FulfillPaidOrderHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cardGateway">Card payment gateway used when the invoice was paid by card.</param>
    /// <param name="pluginResolver">Resolver for hosted-gateway payment plugins.</param>
    /// <param name="bus">Wolverine message bus.</param>
    /// <param name="logger">Structured logger.</param>
    public FulfillPaidOrderHandler(
        IHostpanelDbContext context,
        ICardPaymentGateway cardGateway,
        IPaymentPluginResolver pluginResolver,
        IMessageBus bus,
        ILogger<FulfillPaidOrderHandler> logger)
    {
        _context = context;
        _cardGateway = cardGateway;
        _pluginResolver = pluginResolver;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Handles <see cref="FulfillPaidOrderCommand"/>.</summary>
    /// <param name="cmd">The fulfill command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once fulfillment has run (a non-Pending order is a no-op, and still Success), or
    /// <c>ORDER_NOT_FOUND</c> when the id names no order, <c>ORDER_NOT_INVOICED</c> when the order
    /// carries no linked invoice, or <c>INVOICE_NOT_FOUND</c> when that invoice has gone missing.
    /// Every other failure -- a rejected domain, a provisioning fault -- is caught per line item,
    /// refunded or logged, and never turns the whole command into a refusal.
    /// </returns>
    public async Task<Result> HandleAsync(FulfillPaidOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogInformation(
                "Order {OrderId} is {Status}; fulfillment already ran — skipping.", order.Id, order.Status);
            return Result.Success();
        }

        order.Accept();
        await _context.SaveChangesAsync(ct);

        if (order.InvoiceId is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_INVOICED);
        }

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == order.InvoiceId.Value, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        int? createdDomainId = null;
        int? createdServiceId = null;
        string? orderDomainName = null;

        foreach (var item in order.Items)
        {
            if (item.DomainAction is not null)
            {
                if (item.Domain is null)
                {
                    // Order-item structural invariant: a domain-action item always names a
                    // domain. A genuine data-integrity fault rather than a business refusal --
                    // there is no domain name to refund an order against, so this is logged loud
                    // rather than routed into the refund path below.
                    _logger.LogCritical(
                        "Order {OrderId} item {ItemId} has domain action '{Action}' but no domain " +
                        "name. Order is Paid/Accepted but this item was never provisioned — manual " +
                        "intervention required.",
                        order.Id, item.Id, item.DomainAction);
                    continue;
                }

                var domain = item.Domain;
                var (domainId, failureReason) = await FulfillDomainActionAsync(order, item, domain, ct);

                if (failureReason is not null)
                {
                    // Registrar immediately rejected the order (duplicate domain, invalid TLD,
                    // API error, etc.). Issue automatic refund and notify.
                    await HandleDomainRegistrationFailedAsync(invoice, order.ClientId, domain, failureReason, ct);
                }
                else
                {
                    createdDomainId ??= domainId;
                    orderDomainName = domain;
                }
            }
            else
            {
                try
                {
                    var outcome = await _bus.InvokeAsync<Result<int>>(
                        new OrderServiceCommand(
                            order.ClientId, item.ProductId, item.BillingCycle,
                            item.FirstPaymentAmount, item.RecurringAmount,
                            order.PaymentMethod, item.Domain ?? orderDomainName), ct);

                    if (outcome.IsFailure)
                    {
                        // A paid order with an unprovisioned item is a critical, must-be-found
                        // condition: the client was charged and the order is Accepted, but this
                        // item never became a service, and every later fulfillment retry is a
                        // logged no-op (order.Status is no longer Pending). One failing item must
                        // not abort the rest of the loop, so log loud and keep going.
                        _logger.LogCritical(
                            "Service provisioning failed for order {OrderId} item {ItemId} " +
                            "(product {ProductId}): {Reason}. Order is Paid/Accepted but this " +
                            "item was never provisioned — manual intervention required.",
                            order.Id, item.Id, item.ProductId, DescribeFailure(outcome));
                    }
                    else
                    {
                        createdServiceId = outcome.Value;
                    }
                }
                catch (Exception ex)
                {
                    // Same must-be-found condition as above, reached instead by a genuine
                    // exception out of the dispatch (a fault, not a Result the use case answered
                    // with). One failing item must not abort the rest of the loop.
                    _logger.LogCritical(
                        ex,
                        "Service provisioning failed for order {OrderId} item {ItemId} (product {ProductId}). " +
                        "Order is Paid/Accepted but this item was never provisioned — manual intervention required.",
                        order.Id, item.Id, item.ProductId);
                }
            }
        }

        // Auto-link domain to hosting service when both in same order
        if (createdDomainId.HasValue && createdServiceId.HasValue)
        {
            var domain = await _context.Domains
                .IncludeChildren()
                .FirstOrDefaultAsync(x => x.Id == createdDomainId.Value, ct);
            domain?.LinkService(createdServiceId.Value);
            await _context.SaveChangesAsync(ct);
        }

        return Result.Success();
    }

    /// <summary>
    /// Runs the one domain action a paid order's line item carries.
    /// </summary>
    /// <param name="order">The paid order the line belongs to.</param>
    /// <param name="item">The line item, already known to carry a domain action.</param>
    /// <param name="domain">The domain name from the order line.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new domain's id when this action creates one (register, transfer) and it succeeded;
    /// otherwise <see langword="null"/> for a renewal, an unrecognised action, or a failure. The
    /// failure reason is <see langword="null"/> on success and a human-readable sentence on
    /// failure, for <see cref="HandleDomainRegistrationFailedAsync"/> to refund and log.
    /// </returns>
    private async Task<(int? DomainId, string? FailureReason)> FulfillDomainActionAsync(
        Order order, OrderItem item, string domain, CancellationToken ct)
    {
        if (item.DomainAction == DomainActionNames.Register)
        {
            var outcome = await _bus.InvokeAsync<Result<int>>(
                new RegisterDomainCommand(
                    order.ClientId,
                    domain,
                    item.Years ?? 1,
                    WhoisPrivacy: false,
                    AutoRenew: true,
                    Nameserver1: null,
                    Nameserver2: null,
                    FirstPaymentAmount: item.FirstPaymentAmount,
                    RecurringAmount: item.RecurringAmount,
                    PaymentMethod: order.PaymentMethod), ct);

            return outcome.IsFailure ? (null, DescribeFailure(outcome)) : (outcome.Value, null);
        }

        if (item.DomainAction == DomainActionNames.Transfer)
        {
            if (item.EppCode is null)
            {
                return (null, $"Order {order.Id} item {item.Id} is a transfer for '{domain}' but has no EPP code.");
            }

            var outcome = await _bus.InvokeAsync<Result<int>>(
                new TransferDomainCommand(
                    order.ClientId,
                    domain,
                    item.EppCode,
                    WhoisPrivacy: false,
                    FirstPaymentAmount: item.FirstPaymentAmount,
                    RecurringAmount: item.RecurringAmount,
                    PaymentMethod: order.PaymentMethod), ct);

            return outcome.IsFailure ? (null, DescribeFailure(outcome)) : (outcome.Value, null);
        }

        if (item.DomainAction == DomainActionNames.Renew)
        {
            return (null, await RenewPaidDomainAsync(order, domain, item.Years ?? 1, ct));
        }

        // No action this handler recognises: the order line is accepted as-is (matching the
        // behaviour before this method existed), and the caller still records the domain name
        // for the auto-link check below.
        return (null, null);
    }

    /// <summary>
    /// Extends a domain the paying client already owns, once the renewal invoice is settled.
    /// </summary>
    /// <remarks>
    /// The other two domain actions create a domain and are handed the client id to create it
    /// against. A renewal names one that already exists, so it is looked up by name -- and the
    /// lookup is not scoped by client, which is why the owner is checked here explicitly.
    /// Refusing rather than renewing somebody else's registration matters even though the only
    /// route that raises such an order (<c>RenewMyDomainHandler</c>) checked ownership before
    /// the invoice existed: fulfillment runs later, from a payment webhook, against whatever the
    /// order line says, and a check that ran once at checkout guarantees nothing about the
    /// message that gets replayed here.
    /// </remarks>
    /// <param name="order">The paid order the line belongs to.</param>
    /// <param name="domainName">Fully-qualified name from the order line.</param>
    /// <param name="years">Period bought, in years.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see langword="null"/> once the registrar has extended the registration, or a
    /// human-readable failure reason when no such domain exists, when it belongs to another
    /// client, or when the registrar rejects the renewal -- so the caller's money goes back the
    /// same way a rejected registration's does. A paid line that silently did nothing would be
    /// worse than a failed one.
    /// </returns>
    private async Task<string?> RenewPaidDomainAsync(Order order, string domainName, int years, CancellationToken ct)
    {
        var existing = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Name == domainName, ct);
        if (existing is null)
        {
            return $"Order {order.Id} renews '{domainName}', which is not registered here.";
        }

        if (existing.ClientId != order.ClientId)
        {
            // Logged without naming the owning client: this is a refusal, and the log line that
            // records it should not be the place somebody learns whose domain it was.
            _logger.LogError(
                "Order {OrderId} (client {ClientId}) renews domain {DomainId}, which belongs to "
                + "another client. Refusing and refunding.",
                order.Id, order.ClientId, existing.Id);

            return $"Order {order.Id} renews '{domainName}', which is not this client's domain.";
        }

        var outcome = await _bus.InvokeAsync<Result>(new RenewDomainCommand(existing.Id, years), ct);
        return outcome.IsFailure ? DescribeFailure(outcome) : null;
    }

    /// <summary>Renders a failed <see cref="Result{T}"/> as the one sentence a log line or a refund note needs.</summary>
    /// <typeparam name="T">The value type of a successful result.</typeparam>
    /// <param name="result">The failed outcome to describe.</param>
    /// <returns>The result's message, its code, or a generic sentence -- whichever is present first.</returns>
    private static string DescribeFailure<T>(Result<T> result) =>
        result.Error ?? result.ErrorCode ?? "The operation failed.";

    /// <summary>Renders a failed <see cref="Result"/> as the one sentence a log line or a refund note needs.</summary>
    /// <param name="result">The failed outcome to describe.</param>
    /// <returns>The result's message, its code, or a generic sentence -- whichever is present first.</returns>
    private static string DescribeFailure(Result result) =>
        result.Error ?? result.ErrorCode ?? "The operation failed.";

    /// <summary>
    /// Issues an automatic refund through whichever gateway the invoice was paid with,
    /// records it, and publishes <see cref="DomainRegistrationFailedEvent"/>.
    /// </summary>
    /// <param name="invoice">The invoice that was paid and must be refunded.</param>
    /// <param name="clientId">The client who placed the order.</param>
    /// <param name="domainName">The domain name that failed to register.</param>
    /// <param name="reason">Human-readable reason from the registrar.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleDomainRegistrationFailedAsync(
        Invoice invoice, int clientId, string domainName, string reason, CancellationToken ct)
    {
        _logger.LogError(
            "Domain registration failed for '{DomainName}' (client {ClientId}): {Reason}. Issuing refund.",
            domainName, clientId, reason);

        try
        {
            string refundId;
            string gateway;
            if (invoice.GatewayModule is not null && invoice.GatewayOrderId is not null)
            {
                var plugin = await _pluginResolver.ResolveAsync(invoice.GatewayModule, ct);
                if (plugin is null)
                {
                    _logger.LogCritical(
                        "REFUND FAILED for domain '{DomainName}' (client {ClientId}, invoice {InvoiceId}): " +
                        "payment plugin '{Module}' is not available for the refund. Manual refund required. " +
                        "Original failure: {Reason}",
                        domainName, clientId, invoice.Id, invoice.GatewayModule, reason);
                    return;
                }

                var amountMinor = CurrencyCodes.ToMinorUnits(invoice.Total);
                refundId = await plugin.RefundAsync(invoice.GatewayOrderId, amountMinor, ct);
                gateway = invoice.GatewayModule;
            }
            else
            {
                if (invoice.GatewayTransactionId is null)
                {
                    _logger.LogCritical(
                        "REFUND FAILED for domain '{DomainName}' (client {ClientId}, invoice {InvoiceId}): " +
                        "invoice has no gateway transaction id to refund via the card gateway. Manual refund required. " +
                        "Original failure: {Reason}",
                        domainName, clientId, invoice.Id, reason);
                    return;
                }

                refundId = await _cardGateway.RefundAsync(invoice.GatewayTransactionId, ct);
                gateway = BuiltInPaymentModules.Stripe;
            }

            invoice.AddRefund(
                DateTimeOffset.UtcNow,
                gateway: gateway,
                transactionId: refundId,
                amount: invoice.Total,
                fees: 0,
                notes: $"Automatic refund: domain registration failed for '{domainName}'. Reason: {reason}");
            await _context.SaveChangesAsync(ct);

            await _bus.PublishAsync(new DomainRegistrationFailedEvent(clientId, domainName, invoice.Id, reason));
        }
        catch (Exception refundEx)
        {
            // Refund failed — log at critical level so it gets immediate attention.
            // The domain was never created so no cleanup needed there,
            // but the client was charged and could not be refunded automatically.
            _logger.LogCritical(
                refundEx,
                "REFUND FAILED for domain '{DomainName}' (client {ClientId}, invoice {InvoiceId}). " +
                "Manual refund required. Original failure: {Reason}",
                domainName, clientId, invoice.Id, reason);
        }
    }
}
