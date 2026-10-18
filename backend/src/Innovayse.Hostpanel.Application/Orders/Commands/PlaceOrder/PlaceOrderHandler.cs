using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Billing.Queries.ListAvailablePaymentMethods;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Queries.GetTldPricing;
using Innovayse.Hostpanel.Application.Orders.Common;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Orders.Commands.PlaceOrder;

/// <summary>
/// Places a new order for an existing or newly registered client.
/// Creates the <see cref="Order"/> aggregate, snapshots product prices into line items,
/// generates a linked <see cref="Invoice"/>, and persists everything in a single unit of work.
/// </summary>
public sealed class PlaceOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Creates the account for guest checkout registration.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Role store, for granting the Client role to a newly provisioned guest.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>Wolverine message bus, for invoking the payment-method and TLD pricing queries.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Who is ordering, and from where; the command says neither, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>The currency the ordering client is billed in, stamped on the order's invoice.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>
    /// Logs the precise reason a line item's price could not be resolved -- which TLD, which
    /// currency mismatch -- since the code the caller receives is deliberately generic.
    /// </summary>
    private readonly ILogger<PlaceOrderHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="provisioning">Creates the account for guest checkout registration.</param>
    /// <param name="roles">Role store, for granting the Client role.</param>
    /// <param name="bus">Wolverine message bus for invoking payment-method and TLD pricing queries.</param>
    /// <param name="caller">Who is ordering, and from where; the command says neither, and must not.</param>
    /// <param name="payerCurrency">The currency the ordering client is billed in, stamped on the order's invoice.</param>
    /// <param name="logger">Logs the precise reason a line item's price could not be resolved.</param>
    public PlaceOrderHandler(
        IHostpanelDbContext context,
        IUserProvisioning provisioning,
        ISubjectRoleStore roles,
        IMessageBus bus,
        ICurrentUserService caller,
        IPayerCurrencyResolver payerCurrency,
        ILogger<PlaceOrderHandler> logger)
    {
        _context = context;
        _provisioning = provisioning;
        _roles = roles;
        _bus = bus;
        _caller = caller;
        _payerCurrency = payerCurrency;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The order to place.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new order and invoice ids and the order's payment token, or
    /// <c>ORDER_PAYMENT_METHOD_UNAVAILABLE</c> when the checkout is not offering that method,
    /// <c>ORDER_CURRENCY_UNAVAILABLE</c> when a guest chose a currency this panel does not offer,
    /// <c>ORDER_GUEST_DETAILS_REQUIRED</c> when a guest checkout reached the handler without its
    /// registration fields, <c>PRODUCT_NOT_FOUND</c> or <c>PRODUCT_NOT_AVAILABLE</c> when a line
    /// item names a product that does not exist or is not active,
    /// <c>ORDER_PRODUCT_PRICE_UNAVAILABLE</c> when a product has no price for the requested
    /// currency and cycle, or <c>ORDER_DOMAIN_PRICING_INVALID</c> when a domain line cannot be
    /// priced -- see the private pricing helpers below for the five causes that one code covers.
    /// </returns>
    public async Task<Result<PlaceOrderResultDto>> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // The currency is settled before anything is created: an existing client is billed in
        // theirs, and a client this order is about to create in the checkout's choice, else the
        // base. Knowing it first is what lets a payment method be refused before an account
        // exists for it, and lets a guest who chose AMD be priced in AMD, not the base. Every
        // line below is read from the stored price in this currency; nothing is converted.
        var existingClient = await FindCallersClientAsync(ct);
        var currency = existingClient is null
            ? await ResolveNewClientCurrencyAsync(cmd, ct)
            : Result.Success(await _payerCurrency.ForClientAsync(existingClient.Id, ct));

        if (currency.IsFailure)
        {
            return Result<PlaceOrderResultDto>.FailureCode(currency.ErrorCode!);
        }

        var methodAvailable = await EnsurePaymentMethodAvailableAsync(cmd.PaymentMethod, currency.Value!.Code, ct);
        if (methodAvailable.IsFailure)
        {
            return Result<PlaceOrderResultDto>.FailureCode(methodAvailable.ErrorCode!);
        }

        var clientId = existingClient?.Id;
        if (clientId is null)
        {
            var created = await CreateClientAsync(cmd, currency.Value.Code, ct);
            if (created.IsFailure)
            {
                return Result<PlaceOrderResultDto>.FailureCode(created.ErrorCode!);
            }

            clientId = created.Value;
        }

        // Next after the highest id ever used, not a row count: a deleted order must never free
        // its number back up for reuse, which a count-based scheme would do silently.
        var nextNumber = await _context.Orders.AnyAsync(ct)
            ? await _context.Orders.MaxAsync(o => o.Id, ct) + 1
            : 1;
        var orderNumber = $"ORD-{nextNumber:D4}";

        var productIds = cmd.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Include(p => p.Prices)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);
        var productMap = products.ToDictionary(p => p.Id);

        var order = Order.Create(orderNumber, clientId.Value, cmd.PaymentMethod, _caller.IpAddress);

        // Pre-fetch TLD pricing if any domain items exist. The table is asked for in the payer's
        // currency, so every figure comes back converted from the TLD's sell currency with the
        // stored exchange rate and is the amount the invoice is billed.
        var hasDomainItems = cmd.Items.Any(i => i.DomainAction is not null);
        // The handler answers with a Result, so that is the type the bus is asked for; asking for
        // the bare DTO makes Wolverine look for a handler that returns one and fail the order.
        var pricingOutcome = hasDomainItems
            ? await _bus.InvokeAsync<Result<TldPricingDto>>(new GetTldPricingQuery(currency.Value.Code), ct)
            : null;
        if (pricingOutcome is { IsFailure: true })
        {
            return Result<PlaceOrderResultDto>.FailureCode(pricingOutcome.ErrorCode!);
        }

        var tldPricing = pricingOutcome?.Value;

        foreach (var item in cmd.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                return Result<PlaceOrderResultDto>.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_FOUND);
            }

            if (product.Status != ProductStatus.Active)
            {
                return Result<PlaceOrderResultDto>.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_AVAILABLE);
            }

            var priceOutcome = item.DomainAction is not null
                ? ResolveDomainPrice(tldPricing!, item.Domain!, item.DomainAction, item.Years ?? 1)
                : ResolvePrice(product, item.BillingCycle, currency.Value);
            if (priceOutcome.IsFailure)
            {
                return Result<PlaceOrderResultDto>.FailureCode(priceOutcome.ErrorCode!);
            }

            var price = priceOutcome.Value;

            order.AddItem(item.ProductId, product.Name, item.BillingCycle, price, price,
                item.Domain, item.Hostname, item.DomainAction, item.EppCode, item.Years);
        }

        _context.Orders.Add(order);

        var invoice = Invoice.Create(clientId.Value, DateTimeOffset.UtcNow.AddDays(7), currency.Value.Code);

        foreach (var item in order.Items)
        {
            invoice.AddItem(item.ProductName, item.FirstPaymentAmount, 1);
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(ct);

        order.LinkInvoice(invoice.Id);
        await _context.SaveChangesAsync(ct);

        return Result.Success(new PlaceOrderResultDto(order.Id, invoice.Id, order.PaymentToken));
    }

    /// <summary>
    /// Refuses a payment method the checkout is not offering right now.
    /// </summary>
    /// <remarks>
    /// The method is a string the caller sends, and the list the checkout shows is only a
    /// suggestion to a client that can send anything. Without this check an operator who
    /// switched a gateway off in the admin integrations page still received orders against it:
    /// a bank-transfer order would sit pending for a transfer nobody expects, and a card order
    /// would fail later, on the first call that needed the gateway. The same query the checkout
    /// lists from decides here, so what is shown and what is accepted cannot drift apart.
    /// <para>
    /// The query is asked for the currency this order will bill in rather than the caller's. For
    /// a guest the two differ: the caller has no client yet, so the query would answer for the
    /// base, and a guest who chose AMD could place an order against a USD-only gateway that the
    /// payment start then refuses.
    /// </para>
    /// </remarks>
    /// <param name="paymentMethod">Module id the checkout sent.</param>
    /// <param name="currencyCode">ISO 4217 alpha code the order's invoice will bill in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <see cref="HostpanelErrorCodes.ORDER_PAYMENT_METHOD_UNAVAILABLE"/> when the
    /// method is not on offer.
    /// </returns>
    private async Task<Result> EnsurePaymentMethodAvailableAsync(
        string paymentMethod, string currencyCode, CancellationToken ct)
    {
        var available = await _bus.InvokeAsync<Result<IReadOnlyList<AvailablePaymentMethodDto>>>(
            new ListAvailablePaymentMethodsQuery(currencyCode), ct);

        if (available.IsFailure)
        {
            return Result.FailureCode(available.ErrorCode!);
        }

        if (!available.Value!.Any(m => string.Equals(m.Module, paymentMethod, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_PAYMENT_METHOD_UNAVAILABLE);
        }

        return Result.Success();
    }

    /// <summary>
    /// Finds the client record the caller's credential names, when it names one that exists.
    /// </summary>
    /// <remarks>
    /// A signed-in caller orders for their own account. The subject comes from the credential,
    /// so there is no id anyone could send to order against another account. A caller the
    /// credential names but who has no client record yet, and a guest, both come back as
    /// nothing here and get a record from <see cref="CreateClientAsync"/>.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The caller's client, or <see langword="null"/> when they have none yet.</returns>
    private async Task<Client?> FindCallersClientAsync(CancellationToken ct) =>
        _caller.UserId is { } subject
            ? await _context.Clients.FirstOrDefaultAsync(c => c.UserId == subject, ct)
            : null;

    /// <summary>
    /// Creates the client record the order belongs to, for a caller who has none. A caller the
    /// credential names gets a record against their existing account; anyone else creates a new
    /// account and <see cref="Client"/> record as part of guest checkout.
    ///
    /// <para>
    /// Guest checkout is a local-mode flow. Where an SSO owns the accounts this product
    /// cannot create one, so the provisioner refuses and says so — the customer has to
    /// sign in first, which is the only way their order can belong to anyone.
    /// </para>
    /// </summary>
    /// <param name="cmd">The place order command.</param>
    /// <param name="currencyCode">The currency the new record is billed in, already checked to be on offer.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new client's ID, or <see cref="HostpanelErrorCodes.ORDER_GUEST_DETAILS_REQUIRED"/> when
    /// an anonymous caller's registration fields are missing (the validator already requires these
    /// of an anonymous caller, so this is a defensive answer for a command that reached the
    /// handler some other way), or <c>USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT</c> where an
    /// SSO owns the accounts.
    /// </returns>
    private async Task<Result<int>> CreateClientAsync(PlaceOrderCommand cmd, string currencyCode, CancellationToken ct)
    {
        // A caller the credential names but who has no client record yet is registered against
        // that credential, which is what used to happen when the controller resolved this.
        var subject = _caller.UserId;
        if (subject is not null)
        {
            // Their first order. The account already exists — they authenticated to get here —
            // so what is missing is only this product's client row, and the credential carries
            // everything it needs. Provisioning is deliberately not called: it creates accounts,
            // and where an SSO owns them it refuses, which is correct and not what is wanted
            // here.
            return Result.Success(await CreateClientForCallerAsync(subject, cmd, currencyCode, ct));
        }

        // Anonymous, and guest checkout is the only way left. The validator already requires all
        // four fields of a caller with no subject, so this refusal is reached only when something
        // dispatched the command without going through it.
        if (string.IsNullOrWhiteSpace(cmd.Email) || string.IsNullOrWhiteSpace(cmd.Password))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.ORDER_GUEST_DETAILS_REQUIRED);
        }

        var created = await _provisioning.CreateAsync(
            cmd.Email!, cmd.Password!, cmd.FirstName, cmd.LastName, ct);
        if (created.IsFailure)
        {
            return Result<int>.FailureCode(created.ErrorCode!);
        }

        var userId = created.Value!;
        await _roles.AddAsync(userId, Roles.Client, ct);

        var newClient = Client.Create(userId, cmd.FirstName!, cmd.LastName!, cmd.Email!);
        newClient.SetCurrency(currencyCode);
        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync(ct);

        return Result.Success(newClient.Id);
    }

    /// <summary>
    /// The currency a client record created by this order is billed in: the checkout's choice,
    /// else the base.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the one moment a customer chooses their currency; after the first invoice it is
    /// locked (see <c>UpdateClientHandler</c>). It is recorded explicitly even when the choice
    /// is the base, so the row says what the client was billed in rather than leaving a later
    /// reader to work out what the base was at the time.
    /// </para>
    /// <para>
    /// Only a client this order creates is asked. An existing client's order carries the same
    /// field, because the checkout form sends it either way, and it is ignored: they already
    /// have a currency, and their invoices are in it.
    /// </para>
    /// </remarks>
    /// <param name="cmd">The order being placed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The configured currency to record on the new client and bill the order in, or
    /// <see cref="HostpanelErrorCodes.ORDER_CURRENCY_UNAVAILABLE"/> when the chosen code is not
    /// configured and enabled.
    /// </returns>
    private async Task<Result<Currency>> ResolveNewClientCurrencyAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        if (cmd.Currency is null)
        {
            return Result.Success(await _payerCurrency.BaseAsync(ct));
        }

        var offered = await _context.FindOfferedCurrencyAsync(cmd.Currency, ct);
        return offered is null
            ? Result<Currency>.FailureCode(HostpanelErrorCodes.ORDER_CURRENCY_UNAVAILABLE)
            : Result.Success(offered);
    }

    /// <summary>
    /// Creates the client row for a signed-in caller who does not have one yet.
    /// </summary>
    /// <remarks>
    /// Names come from the order when it carries them and from the credential otherwise. The
    /// checkout form asks a signed-in caller for neither, so in practice it is the credential;
    /// the command is preferred anyway because a caller who did type a billing name meant it.
    /// <para>
    /// The email is read from the credential rather than the command on purpose. A caller can
    /// put any address in a form field, and this row decides which account every later invoice
    /// and service belongs to.
    /// </para>
    /// </remarks>
    /// <param name="subject">The caller's subject, already known to have no client row.</param>
    /// <param name="cmd">The order being placed.</param>
    /// <param name="currencyCode">The currency the new row is billed in, already checked to be on offer.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new client's ID.</returns>
    private async Task<int> CreateClientForCallerAsync(
        string subject, PlaceOrderCommand cmd, string currencyCode, CancellationToken ct)
    {
        var (credentialFirst, credentialLast) = SplitDisplayName(_caller.UserName);

        var client = Client.Create(
            subject,
            FirstNonBlank(cmd.FirstName, credentialFirst) ?? string.Empty,
            FirstNonBlank(cmd.LastName, credentialLast) ?? string.Empty,
            _caller.Email ?? cmd.Email ?? string.Empty);

        // A new row, so this order is the one that chooses its currency — the same rule as a
        // guest's, because the caller is choosing at the same checkout.
        client.SetCurrency(currencyCode);

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);

        return client.Id;
    }

    /// <summary>Returns the first of two values that is neither null nor blank.</summary>
    /// <param name="preferred">The value to use when it says something.</param>
    /// <param name="fallback">Used when <paramref name="preferred"/> does not.</param>
    /// <returns>The chosen value, or <see langword="null"/> when neither says anything.</returns>
    private static string? FirstNonBlank(string? preferred, string? fallback) =>
        string.IsNullOrWhiteSpace(preferred) ? (string.IsNullOrWhiteSpace(fallback) ? null : fallback) : preferred;

    /// <summary>
    /// Splits a credential's display name into the first and last name the client row stores.
    /// </summary>
    /// <remarks>
    /// A display name is one string and this schema wants two, so the split is a guess: the
    /// first word is the given name and whatever follows is the family name. It is wrong for
    /// some names and there is no parse that is right for all of them. It is used only to fill
    /// the row at creation — the customer can correct both fields in their profile, and nothing
    /// but display reads them.
    /// </remarks>
    /// <param name="displayName">The credential's name claim, if it carries one.</param>
    /// <returns>The first and last name, each null when the name does not supply it.</returns>
    private static (string? First, string? Last) SplitDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (null, null);
        }

        var parts = displayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return (parts[0], parts.Length > 1 ? parts[1] : null);
    }

    /// <summary>Reads the stored price for a product in the payer's currency and cycle.</summary>
    /// <remarks>
    /// A product with no price in that currency is not for sale to that payer; the order refuses
    /// rather than converting from another currency, because a converted figure is not a price
    /// the operator set.
    /// </remarks>
    /// <param name="product">The product.</param>
    /// <param name="billingCycle">The requested cycle string.</param>
    /// <param name="currency">The payer's currency.</param>
    /// <returns>
    /// The amount, or a failed <see cref="Result{T}"/> carrying
    /// <c>ORDER_PRODUCT_PRICE_UNAVAILABLE</c> when the product has no price for that cycle in that
    /// currency.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the billing cycle is not recognised.</exception>
    private Result<decimal> ResolvePrice(Product product, string billingCycle, Currency currency)
    {
        var cycle = BillingCycleParser.Parse(billingCycle);
        var price = product.PriceFor(currency.Code, cycle);
        if (price is null)
        {
            _logger.LogWarning(
                "'{ProductName}' has no {Cycle} price in {CurrencyCode}; it cannot be ordered in that currency.",
                product.Name, cycle, currency.Code);
            return Result<decimal>.FailureCode(HostpanelErrorCodes.ORDER_PRODUCT_PRICE_UNAVAILABLE);
        }

        return Result<decimal>.Success(price.Value);
    }

    /// <summary>
    /// Resolves the price for a domain registration or transfer from the TLD pricing table.
    /// </summary>
    /// <param name="tldPricing">Pre-fetched TLD pricing data, already converted to the payer's currency.</param>
    /// <param name="domainName">Fully-qualified domain name (e.g. "example.com").</param>
    /// <param name="action">Domain action: "register", "transfer" or "renew".</param>
    /// <param name="years">Registration period in years.</param>
    /// <returns>
    /// The validated price for the domain operation, or a failed <see cref="Result{T}"/> carrying
    /// <c>ORDER_DOMAIN_PRICING_INVALID</c> when the name has no TLD, the TLD is not supported, the action is unrecognised, or there is no
    /// price for the period.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A TLD has one stored sell currency; the pricing query converts its figures to the payer's
    /// currency with the stored exchange rates, so the figure read here is already the amount the
    /// payer is charged and the invoice carries the same currency. The TLD's own sell currency
    /// therefore no longer gates the order.
    /// </para>
    /// <para>
    /// "renew" prices from the TLD table's renewal column, which has always been loaded and
    /// published to the public pricing page and had no order path behind it. It is here so a
    /// client-initiated renewal is a purchase like the other two rather than a free call to the
    /// registrar; see <c>RenewMyDomainHandler</c>.
    /// </para>
    /// </remarks>
    private Result<decimal> ResolveDomainPrice(
        TldPricingDto tldPricing, string domainName, string action, int years)
    {
        var dotIndex = domainName.IndexOf('.');
        if (dotIndex < 0)
        {
            _logger.LogWarning("Invalid domain name on an order line: {DomainName}", domainName);
            return Result<decimal>.FailureCode(HostpanelErrorCodes.ORDER_DOMAIN_PRICING_INVALID);
        }

        var tld = domainName[(dotIndex + 1)..];

        if (!tldPricing.Pricing.TryGetValue(tld, out var entry))
        {
            _logger.LogWarning("TLD '.{Tld}' is not supported for domain {Action}.", tld, action);
            return Result<decimal>.FailureCode(HostpanelErrorCodes.ORDER_DOMAIN_PRICING_INVALID);
        }

        var priceMap = action.ToLowerInvariant() switch
        {
            DomainActionNames.Register => entry.Register,
            DomainActionNames.Transfer => entry.Transfer,
            DomainActionNames.Renew => entry.Renew,
            _ => null,
        };

        if (priceMap is null)
        {
            _logger.LogWarning("Unsupported domain action: {Action}.", action);
            return Result<decimal>.FailureCode(HostpanelErrorCodes.ORDER_DOMAIN_PRICING_INVALID);
        }

        var yearKey = years.ToString();
        if (!priceMap.TryGetValue(yearKey, out var priceStr) || !decimal.TryParse(priceStr, out var price))
        {
            _logger.LogWarning("No pricing available for '.{Tld}' {Action} for {Years} year(s).", tld, action, years);
            return Result<decimal>.FailureCode(HostpanelErrorCodes.ORDER_DOMAIN_PRICING_INVALID);
        }

        return Result<decimal>.Success(price);
    }
}
