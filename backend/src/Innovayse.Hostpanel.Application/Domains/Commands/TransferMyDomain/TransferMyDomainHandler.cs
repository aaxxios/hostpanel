using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Orders.Commands.PlaceOrder;
using Innovayse.Hostpanel.Application.Orders.Common;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Domains.Commands.TransferMyDomain;
/// <summary>
/// Places a paid transfer-in order for a domain the calling client is bringing to this
/// registrar, and never lets the caller say whose account it lands on.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is an order and not a direct registrar call.</b> The obvious mirror of
/// <c>RenewMyDomainHandler</c> would be to settle the caller and dispatch
/// <c>TransferDomainCommand</c> straight away. That command calls the registrar immediately and
/// records a price on the new domain without ever raising an invoice -- which is correct for the
/// admin route it was written for, where a person has already decided the transfer is warranted,
/// and wrong for a self-service one, where it would hand out transfers for free.
/// </para>
/// <para>
/// A transfer-in is already a thing this platform sells: <c>PlaceOrderCommand</c> accepts an
/// item with <c>DomainAction = "transfer"</c>, prices it from the TLD table, raises the invoice,
/// and <c>FulfillPaidOrderHandler</c> dispatches <c>TransferDomainCommand</c> once that invoice
/// is paid -- refunding automatically if the registrar rejects it. So the shared use case this
/// handler reaches for is the order, not the registrar, and everything downstream of payment is
/// the code the public checkout already runs.
/// </para>
/// <para>
/// <b>Where the ownership boundary is.</b> <see cref="PlaceOrderCommand"/> carries no client id
/// either; it resolves the account from the credential inside its own handler. This one calls
/// <see cref="ICurrentUserService.RequireUserId"/> before dispatching anyway, and not for
/// decoration: <c>PlaceOrderHandler</c> treats "no credential" as guest checkout and would
/// register a brand-new account for an anonymous caller. That is a sensible answer at a public
/// checkout and a nonsensical one behind <c>/api/me/domains</c>, so this refuses instead.
/// </para>
/// </remarks>
public sealed class TransferMyDomainHandler
{
    /// <summary>Billing cycle recorded on a domain order line.</summary>
    /// <remarks>
    /// Domain lines are priced from the TLD table by action and year count, not from the
    /// product's cycle prices, so this is a label rather than an input to the total. It is the
    /// same string the public cart puts on a domain item, and the order validator requires a
    /// non-empty one.
    /// </remarks>
    private const string DomainBillingCycle = BillingCycleNames.Annually;

    /// <summary>The data context, for the domain product lookup.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is transferring; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Wolverine bus, used to reach the order use case once the caller is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<TransferMyDomainHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context, for the domain product lookup.</param>
    /// <param name="caller">Who is transferring; the command does not say, and must not.</param>
    /// <param name="bus">Wolverine bus, used to reach the order use case once the caller is settled.</param>
    /// <param name="logger">Structured logger.</param>
    public TransferMyDomainHandler(
        IHostpanelDbContext context,
        ICurrentUserService caller,
        IMessageBus bus,
        ILogger<TransferMyDomainHandler> logger)
    {
        _context = context;
        _caller = caller;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Handles <see cref="TransferMyDomainCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this orders against the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new order and the invoice raised for it, for the client to pay, or
    /// <c>DOMAIN_PRODUCT_NOT_CONFIGURED</c> when this installation sells no domain product.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the request carries no subject. Deliberately not allowed to fall through to
    /// <c>PlaceOrderHandler</c>, which would read the absence of a credential as guest checkout.
    /// </exception>
    public async Task<Result<PlaceOrderResultDto>> HandleAsync(TransferMyDomainCommand cmd, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        var domainProductOutcome = await FindDomainProductAsync(ct);
        if (domainProductOutcome.IsFailure)
        {
            return Result<PlaceOrderResultDto>.FailureCode(domainProductOutcome.ErrorCode!);
        }

        _logger.LogInformation(
            "Placing a domain transfer-in order for '{DomainName}' on behalf of subject {UserId}.",
            cmd.DomainName, userId);

        // Every registration field is null on purpose. This route is behind the Client role, so
        // there is a credential; PlaceOrderHandler resolves the account from it and the guest
        // branch is unreachable. Passing names or an email here would be inventing a second way
        // to say who is ordering, which is the thing being avoided.
        return await _bus.InvokeAsync<Result<PlaceOrderResultDto>>(
            new PlaceOrderCommand(
                FirstName: null,
                LastName: null,
                Email: null,
                Password: null,
                PhoneNumber: null,
                PaymentMethod: cmd.PaymentMethod,
                Items:
                [
                    new PlaceOrderItemDto(
                        ProductId: domainProductOutcome.Value!.Id,
                        BillingCycle: DomainBillingCycle,
                        Domain: cmd.DomainName,
                        Hostname: null,
                        DomainAction: DomainActionNames.Transfer,
                        EppCode: cmd.EppCode,
                        Years: cmd.Years),
                ]),
            ct);
    }

    /// <summary>
    /// Finds the active product a domain order line hangs off.
    /// </summary>
    /// <remarks>
    /// Keyed on <see cref="ProductType.Domain"/>, which is what the type is for. The public
    /// domain-search page picks this product by looking for "domain" in the name, and that is a
    /// guess -- an operator who calls the product "Domain names" and an operator who calls a
    /// hosting plan "Domain + Hosting" get different answers from it. Nothing new should be
    /// written that way, so this reads the flag instead.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The domain product to order against, or <c>DOMAIN_PRODUCT_NOT_CONFIGURED</c> when this
    /// installation has no active domain product, so there is nothing to sell a transfer as -- a
    /// configuration gap rather than a caller error, but the caller still needs a sentence
    /// rather than a 500.
    /// </returns>
    private async Task<Result<Product>> FindDomainProductAsync(CancellationToken ct)
    {
        // Was IProductRepository.ListAsync(groupId: null, activeOnly: true), which ordered by
        // name -- preserved here so a caller with several active domain products still gets the
        // one that used to sort first. Read-only, so no tracking is needed.
        var domainProduct = await _context.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active && p.Type == ProductType.Domain)
            .OrderBy(p => p.Name)
            .FirstOrDefaultAsync(ct);
        return domainProduct is null
            ? Result<Product>.FailureCode(HostpanelErrorCodes.DOMAIN_PRODUCT_NOT_CONFIGURED)
            : Result.Success(domainProduct);
    }
}
