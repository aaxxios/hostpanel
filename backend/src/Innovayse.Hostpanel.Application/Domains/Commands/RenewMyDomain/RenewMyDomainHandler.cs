using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
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
namespace Innovayse.Hostpanel.Application.Domains.Commands.RenewMyDomain;

/// <summary>
/// Places a paid renewal order for one of the calling client's own domains, and never lets the
/// caller say whose domain it is.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is an order and not a direct registrar call.</b> This handler used to settle
/// ownership and then dispatch <c>RenewDomainCommand</c>, which calls the registrar immediately,
/// extends the expiry date and raises no invoice. That is correct for the admin route it was
/// written for -- a person has already decided the renewal is warranted, and billing is handled
/// outside it -- and wrong for a self-service one, where it hands out paid renewals for free to
/// anyone who owns a domain. It is the same objection that made domain transfer-in an order, and
/// the answer is the same one.
/// </para>
/// <para>
/// A renewal is already a thing this platform prices: the TLD table carries a renewal column
/// beside registration and transfer, and the public pricing page has published it all along.
/// What was missing was an order path behind it. <c>PlaceOrderHandler</c> now accepts an item
/// with <c>DomainAction = "renew"</c>, prices it from that column, raises the invoice, and
/// <c>FulfillPaidOrderHandler</c> dispatches <c>RenewDomainCommand</c> once the invoice is paid
/// -- refunding automatically if the registrar rejects it, the same as for a registration.
/// </para>
/// <para>
/// <b>Where the ownership boundary is.</b> Two checks, deliberately, and neither is redundant.
/// <see cref="IDomainOwnership.RequireOwnedByCallerAsync"/> runs here so the domain named on the
/// route is the caller's before an order number is spent on it; <c>FulfillPaidOrderHandler</c>
/// re-checks the owner when the invoice is paid, because fulfillment runs later from a payment
/// webhook and this check will not have travelled with the order line.
/// <see cref="ICurrentUserService.RequireUserId"/> is called as well -- not for decoration:
/// <c>PlaceOrderHandler</c> treats "no credential" as guest checkout and would register a
/// brand-new account for an anonymous caller, which is a sensible answer at a public checkout
/// and a nonsensical one behind <c>/api/me/domains</c>. In practice the ownership check gets
/// there first; this makes the guarantee independent of that ordering.
/// </para>
/// </remarks>
public sealed class RenewMyDomainHandler
{
    /// <summary>Billing cycle recorded on a domain order line.</summary>
    /// <remarks>
    /// Domain lines are priced from the TLD table by action and year count, not from the
    /// product's cycle prices, so this is a label rather than an input to the total. It is the
    /// same string the public cart and the transfer-in route put on a domain item, and the order
    /// validator requires a non-empty one.
    /// </remarks>
    private const string DomainBillingCycle = BillingCycleNames.Annually;

    /// <summary>The rule that says a client may only touch their own domains.</summary>
    private readonly IDomainOwnership _ownership;

    /// <summary>The data context, for the name the order line has to carry and the domain product lookup.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is renewing; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Wolverine bus, used to reach the order use case once ownership is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<RenewMyDomainHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own domains.</param>
    /// <param name="context">The data context, for the name the order line has to carry and the domain product lookup.</param>
    /// <param name="caller">Who is renewing; the command does not say, and must not.</param>
    /// <param name="bus">Wolverine bus, used to reach the order use case once ownership is settled.</param>
    /// <param name="logger">Structured logger.</param>
    public RenewMyDomainHandler(
        IDomainOwnership ownership,
        IHostpanelDbContext context,
        ICurrentUserService caller,
        IMessageBus bus,
        ILogger<RenewMyDomainHandler> logger)
    {
        _ownership = ownership;
        _context = context;
        _caller = caller;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Handles <see cref="RenewMyDomainCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this orders against the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new order and the invoice raised for it, for the client to pay; <c>DOMAIN_NOT_FOUND</c>
    /// when the domain is not the caller's, when no such domain exists, or when the caller has no
    /// client record; or <c>DOMAIN_PRODUCT_NOT_CONFIGURED</c> when this installation sells no
    /// domain product.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the request carries no subject. Deliberately not allowed to fall through to
    /// <c>PlaceOrderHandler</c>, which would read the absence of a credential as guest checkout.
    /// </exception>
    public async Task<Result<PlaceOrderResultDto>> HandleAsync(RenewMyDomainCommand cmd, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(cmd.DomainId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<PlaceOrderResultDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        // Loaded after the ownership check, never before it: the name is the one thing about
        // somebody else's domain this route could be used to read, and the check is what makes
        // reaching this line safe.
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId, ct);
        if (domain is null)
        {
            // Unreachable in practice -- ownership just confirmed the row -- but kept as a
            // refusal rather than an assumption, answering the same code as everywhere else on
            // this surface.
            return Result<PlaceOrderResultDto>.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var domainProductOutcome = await FindDomainProductAsync(ct);
        if (domainProductOutcome.IsFailure)
        {
            return Result<PlaceOrderResultDto>.FailureCode(domainProductOutcome.ErrorCode!);
        }

        _logger.LogInformation(
            "Placing a domain renewal order for domain {DomainId} on behalf of subject {UserId}.",
            cmd.DomainId, userId);

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
                        Domain: domain.Name,
                        Hostname: null,
                        DomainAction: DomainActionNames.Renew,
                        EppCode: null,
                        Years: cmd.Years),
                ]),
            ct);
    }

    /// <summary>
    /// Finds the active product a domain order line hangs off.
    /// </summary>
    /// <remarks>
    /// Keyed on <see cref="ProductType.Domain"/>, which is what the type is for -- the same
    /// lookup <c>TransferMyDomainHandler</c> makes, and deliberately not the public search
    /// page's guess at a product whose name contains "domain".
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The domain product to order against, or <c>DOMAIN_PRODUCT_NOT_CONFIGURED</c> when this
    /// installation has no active domain product, so there is nothing to sell a renewal as -- a
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
