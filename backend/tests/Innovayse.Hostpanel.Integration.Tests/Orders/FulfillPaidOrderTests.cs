using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Orders.Commands.FulfillPaidOrder;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wolverine;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <see cref="FulfillPaidOrderHandler"/> against Postgres: the not-found and not-invoiced
/// refusals, the idempotent no-op, the plain-service happy path and the automatic refund a
/// rejected domain registration triggers.
/// </summary>
/// <remarks>
/// The handler is built by hand from a DI scope rather than resolved from one, the same way
/// <c>CompleteGatewayPaymentTests</c> does for its own concurrency cases: Wolverine invokes
/// handlers itself and does not register them as resolvable services.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class FulfillPaidOrderTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds the handler over a DI scope's real ports, with the gateway plugin resolver faked.</summary>
    /// <param name="scope">The scope to resolve the context, card payment gateway service and bus from.</param>
    /// <param name="pluginResolver">The fake plugin resolver to use for the refund path.</param>
    /// <returns>The handler under test.</returns>
    private static FulfillPaidOrderHandler BuildHandler(IServiceScope scope, FakePaymentPluginResolver pluginResolver) =>
        new(
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<ICardPaymentGateway>(),
            pluginResolver,
            scope.ServiceProvider.GetRequiredService<IMessageBus>(),
            NullLogger<FulfillPaidOrderHandler>.Instance);

    /// <summary>Seeds a client and an active hosting product priced in USD.</summary>
    /// <param name="context">The context to seed through.</param>
    /// <returns>The client id and product id.</returns>
    private static async Task<(int ClientId, int ProductId)> SeedClientAndProductAsync(IHostpanelDbContext context)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);

        var group = ProductGroup.Create($"fulfill-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(
            group.Id, "Starter Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);

        return (client.Id, product.Id);
    }

    /// <summary>Seeds a Pending order with a linked, unpaid invoice.</summary>
    /// <param name="context">The context to seed through.</param>
    /// <param name="clientId">The owning client.</param>
    /// <param name="configureItems">Adds the order's line items.</param>
    /// <returns>The order id.</returns>
    private static async Task<int> SeedPendingOrderAsync(
        IHostpanelDbContext context, int clientId, Action<Order> configureItems)
    {
        var order = Order.Create($"ORD-{Guid.NewGuid():N}"[..12], clientId, "bank-gateway", ipAddress: null, currency: "USD");
        configureItems(order);
        context.Orders.Add(order);

        var invoice = Invoice.Create(clientId, DateTimeOffset.UtcNow.AddDays(7), "USD");
        invoice.AddItem("Order", 25m, 1);
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(CancellationToken.None);

        order.LinkInvoice(invoice.Id);
        await context.SaveChangesAsync(CancellationToken.None);

        return order.Id;
    }

    /// <summary>An id nothing names is refused as <c>ORDER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task MissingOrderIsRefused()
    {
        using var scope = factory.Services.CreateScope();
        var handler = BuildHandler(scope, new FakePaymentPluginResolver());

        var result = await handler.HandleAsync(new FulfillPaidOrderCommand(987_654_321), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(HostpanelErrorCodes.ORDER_NOT_FOUND);
    }

    /// <summary>An order that already ran fulfilment (no longer Pending) is a no-op, and still succeeds.</summary>
    [Fact]
    public async Task AlreadyAcceptedOrderIsANoOp()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAndProductAsync(context);
        var orderId = await SeedPendingOrderAsync(
            context, clientId,
            order => order.AddItem(productId, "Starter Hosting", BillingCycleNames.Monthly, 10m, 10m, domain: null, hostname: null));
        var order = await context.Orders.FindAsync(orderId);
        order!.Accept();
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = BuildHandler(scope, new FakePaymentPluginResolver());
        var result = await handler.HandleAsync(new FulfillPaidOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>A Pending order with a plain hosting line is accepted and provisions a client service.</summary>
    [Fact]
    public async Task PendingOrderWithAServiceItemAcceptsAndCreatesTheService()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAndProductAsync(context);
        var orderId = await SeedPendingOrderAsync(
            context, clientId,
            order => order.AddItem(productId, "Starter Hosting", BillingCycleNames.Monthly, 10m, 10m, domain: null, hostname: null));

        var handler = BuildHandler(scope, new FakePaymentPluginResolver());
        var result = await handler.HandleAsync(new FulfillPaidOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var reloaded = await context.Orders.FindAsync(orderId);
        reloaded!.Status.Should().Be(OrderStatus.Active);

        var created = await context.ClientServices
            .Where(s => s.ClientId == clientId)
            .ToListAsync(CancellationToken.None);
        created.Should().ContainSingle(s => s.ProductId == productId);
    }

    /// <summary>
    /// A registration line naming a domain already registered here is refused by the registrar
    /// gate before any network call, and the invoice's gateway payment is refunded automatically.
    /// </summary>
    [Fact]
    public async Task DomainAlreadyRegisteredRefundsThePluginPayment()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, _) = await SeedClientAndProductAsync(context);

        const string DomainName = "already-taken.example";
        context.Domains.Add(RegisteredDomain.Register(
            clientId: 999_999, DomainName, DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false));
        await context.SaveChangesAsync(CancellationToken.None);

        var orderId = await SeedPendingOrderAsync(
            context, clientId,
            order => order.AddItem(
                9, DomainName, BillingCycleNames.Annual, 25m, 25m, domain: DomainName, hostname: null,
                domainAction: DomainActionNames.Register, years: 1));

        var order = await context.Orders.FindAsync(orderId);
        var invoice = await context.Invoices.FindAsync(order!.InvoiceId!.Value);
        invoice!.SetGatewaySession("bank-gateway", "gw-order-refund-1");
        invoice.MarkPaidViaGateway("pre-refund-txn");
        await context.SaveChangesAsync(CancellationToken.None);

        var plugin = new FakePaymentPlugin { RefundResult = "plugin-refund-1" };
        var resolver = new FakePaymentPluginResolver();
        resolver.Register("bank-gateway", plugin);

        var handler = BuildHandler(scope, resolver);
        var result = await handler.HandleAsync(new FulfillPaidOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        plugin.LastRefundGatewayOrderId.Should().Be("gw-order-refund-1");

        var reloadedInvoice = await context.Invoices.FindAsync(invoice.Id);
        reloadedInvoice!.Status.Should().Be(InvoiceStatus.Refunded);
    }
}
