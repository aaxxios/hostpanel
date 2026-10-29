using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Services.Commands.OrderService;

/// <summary>Command to order a new service for a client.</summary>
/// <param name="ClientId">The client placing the order.</param>
/// <param name="ProductId">The product being ordered.</param>
/// <param name="BillingCycle">The billing cycle.</param>
/// <param name="FirstPaymentAmount">
/// One-time amount charged on the first invoice. <see langword="null"/> means "read it from the
/// product's price in the client's currency"; <c>0</c> is a genuinely free first period (a
/// promotion, a free tier) as snapshotted on the order line, and is honoured as such.
/// </param>
/// <param name="RecurringAmount">
/// Amount charged on each renewal invoice. <see langword="null"/> means "read it from the
/// product's price in the client's currency"; <c>0</c> is a genuinely free renewal.
/// </param>
/// <param name="PaymentMethod">Payment method used (e.g. a card gateway name, "BankTransfer").</param>
/// <param name="Domain">Domain name to pre-fill, when purchased alongside hosting.</param>
/// <param name="FirstInvoiceId">
/// The order's invoice the first payment is charged on, or <see langword="null"/> when the service
/// is ordered without one. A client may set the service up only once this invoice is paid in full.
/// </param>
public sealed record OrderServiceCommand(int ClientId, int ProductId, BillingCycle BillingCycle,
    decimal? FirstPaymentAmount, decimal? RecurringAmount, string? PaymentMethod = null,
    string? Domain = null, int? FirstInvoiceId = null);
