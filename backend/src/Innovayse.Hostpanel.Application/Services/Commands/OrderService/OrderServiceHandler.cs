using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Services.Commands.OrderService;
/// <summary>
/// Creates a pending <see cref="ClientService"/> record for the ordered product.
/// Provisioning is handled asynchronously by an event handler that listens
/// for <c>ClientServiceCreatedEvent</c>.
/// </summary>
public sealed class OrderServiceHandler
{
    /// <summary>The module's data context, the new record is added to.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>The currency the ordering client is billed in, for the fallback price.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, the new record is added to.</param>
    /// <param name="uow">Unit of work for persisting changes.</param>
    /// <param name="payerCurrency">The currency the ordering client is billed in, for the fallback price.</param>
    public OrderServiceHandler(
        IHostpanelDbContext context,
        IUnitOfWork uow,
        IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _uow = uow;
        _payerCurrency = payerCurrency;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The client, product, cycle and amounts to order.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The newly created client service id, or <c>PRODUCT_NOT_FOUND</c> when the id names no
    /// product, or <c>PRODUCT_NOT_AVAILABLE</c> when the product is not active or an amount was
    /// left for this handler to price and the product has no price for that cycle in the
    /// client's currency.
    /// </returns>
    public async Task<Result<int>> HandleAsync(OrderServiceCommand cmd, CancellationToken ct)
    {
        // Was IProductRepository.FindByIdAsync, which always included the per-currency prices --
        // preserved here since ResolveCyclePriceAsync below reads them through PriceFor.
        // Read-only, so no tracking is needed.
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProductId, ct);
        if (product is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_FOUND);
        }

        if (product.Status != ProductStatus.Active)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_AVAILABLE);
        }

        // The order-fulfilment callers pass the amounts snapshotted on the order line — zero
        // included, which is a free period and stays free. The client's own route passes null and
        // the amounts come from the product's price in that client's currency. Only a null amount
        // consults the stored price, so a product with no price in that currency is refused
        // rather than sold for nothing, and an accepted order never fails fulfilment over it.
        var needsPrice = cmd.FirstPaymentAmount is null || cmd.RecurringAmount is null;
        var cyclePriceOutcome = needsPrice
            ? await ResolveCyclePriceAsync(product, cmd, ct)
            : Result<decimal>.Success(0m);
        if (cyclePriceOutcome.IsFailure)
        {
            return Result<int>.FailureCode(cyclePriceOutcome.ErrorCode!);
        }

        var firstPayment = cmd.FirstPaymentAmount ?? cyclePriceOutcome.Value;
        var recurring = cmd.RecurringAmount ?? cyclePriceOutcome.Value;

        var service = ClientService.Create(cmd.ClientId, cmd.ProductId, cmd.BillingCycle);

        service.Update(
            domain: cmd.Domain, dedicatedIp: null, username: null,
            password: null, billingCycle: cmd.BillingCycle,
            recurringAmount: recurring, paymentMethod: cmd.PaymentMethod,
            nextRenewalAt: null, subscriptionId: null,
            overrideAutoSuspend: false, suspendUntil: null,
            autoTerminateEndOfCycle: false, autoTerminateReason: null,
            adminNotes: null, provisioningRef: null,
            firstPaymentAmount: firstPayment,
            promotionCode: null, terminatedAt: null,
            serverId: null, quantity: 1, productId: null);

        // Was IClientServiceRepository.Add.
        _context.ClientServices.Add(service);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(service.Id);
    }

    /// <summary>Reads the product's stored price for the ordered cycle in the client's currency.</summary>
    /// <param name="product">The product being ordered.</param>
    /// <param name="cmd">The order command, naming the client and the cycle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The amount, or <c>PRODUCT_NOT_AVAILABLE</c> when the product has no such price.</returns>
    private async Task<Result<decimal>> ResolveCyclePriceAsync(
        Product product, OrderServiceCommand cmd, CancellationToken ct)
    {
        var currency = await _payerCurrency.ForClientAsync(cmd.ClientId, ct);
        var cycle = BillingCycleParser.Parse(cmd.BillingCycle);
        var price = product.PriceFor(currency.Code, cycle);
        return price is null
            ? Result<decimal>.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_AVAILABLE)
            : Result<decimal>.Success(price.Value);
    }
}
