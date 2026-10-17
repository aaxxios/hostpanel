using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Seeding and authentication helpers shared by every Orders use-case test file, extracted from
/// the endpoint-grouped files this A7 Task 6 batch split apart so the seeding logic exists exactly
/// once rather than once per use case.
/// </summary>
public static class OrdersTestSeeds
{
    /// <summary>Shared test password for every seeded test user.</summary>
    public const string TestPassword = "Pass@123!";

    /// <summary>Builds a client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The factory whose admin credentials to mint a token from.</param>
    /// <returns>An authenticated admin <see cref="HttpClient"/>.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await factory.GetAdminTokenAsync());
        return client;
    }

    /// <summary>
    /// Registers a client user over HTTP and authenticates a client carrying their bearer token —
    /// the "wrong role" caller of an admin-only order action.
    /// </summary>
    /// <param name="factory">The factory to mint a client token from.</param>
    /// <returns>An authenticated, non-staff <see cref="HttpClient"/>.</returns>
    public static async Task<HttpClient> ClientCallerAsync(IntegrationTestFactory factory)
    {
        var email = $"orders-{Guid.NewGuid():N}@example.test";
        var httpClient = factory.CreateClient();
        var registered = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Orders",
            lastName = "Caller",
        });
        registered.EnsureSuccessStatusCode();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await factory.GetClientTokenAsync(email, TestPassword));
        return httpClient;
    }

    /// <summary>Seeds a client and an active hosting product, and a pending order for them with one plain line.</summary>
    /// <param name="factory">The factory whose database to seed against.</param>
    /// <param name="status">The status to leave the seeded order in.</param>
    /// <returns>The new order's primary key.</returns>
    public static async Task<int> SeedOrderAsync(IntegrationTestFactory factory, OrderStatus status = OrderStatus.Pending)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);

        var group = ProductGroup.Create($"orders-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        var product = Product.Create(
            group.Id, "Starter Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var order = Order.Create($"ORD-{Guid.NewGuid():N}"[..12], client.Id, "bank_transfer", ipAddress: null);
        order.AddItem(product.Id, "Starter Hosting", BillingCycleNames.Monthly, 10m, 10m, domain: null, hostname: null);

        if (status == OrderStatus.Cancelled)
        {
            order.Cancel();
        }
        else if (status == OrderStatus.Active)
        {
            order.Accept();
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.Id;
    }

    /// <summary>
    /// Seeds a Pending order whose single line item names a product that is then deleted, so
    /// accepting it reaches the same <c>PRODUCT_NOT_FOUND</c> refusal <c>OrderServiceHandler</c>
    /// gives any other line that names no product.
    /// </summary>
    /// <param name="factory">The factory whose database to seed against.</param>
    /// <returns>The new order's primary key.</returns>
    public static async Task<int> SeedOrderWithDeletedProductAsync(IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);

        var group = ProductGroup.Create($"orders-deleted-product-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        var product = Product.Create(
            group.Id, "Vanishing Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var order = Order.Create($"ORD-{Guid.NewGuid():N}"[..12], client.Id, "bank_transfer", ipAddress: null);
        order.AddItem(product.Id, "Vanishing Hosting", BillingCycleNames.Monthly, 10m, 10m, domain: null, hostname: null);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return order.Id;
    }

    /// <summary>Seeds an active hosting product priced in USD.</summary>
    /// <param name="factory">The factory whose database to seed against.</param>
    /// <returns>The new product's primary key.</returns>
    public static async Task<int> SeedProductAsync(IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = ProductGroup.Create($"orders-product-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        var product = Product.Create(
            group.Id, "Starter Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        product.SetPrice("USD", BillingCycle.Monthly, 19.99m);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.Id;
    }

    /// <summary>Seeds a client, a Pending order with one line, and its linked unpaid invoice.</summary>
    /// <param name="factory">The factory whose database the pair is seeded in.</param>
    /// <returns>The order id, its payment token and its linked invoice id.</returns>
    public static async Task<(int OrderId, string PaymentToken, int InvoiceId)> SeedOrderWithInvoiceAsync(
        IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var order = Order.Create($"ORD-{Guid.NewGuid():N}"[..12], client.Id, "bank-gateway", ipAddress: null);
        order.AddItem(1, "Starter Hosting", BillingCycleNames.Monthly, 25m, 25m, domain: null, hostname: null);
        db.Orders.Add(order);

        var invoice = Invoice.Create(client.Id, DateTimeOffset.UtcNow.AddDays(7), "USD");
        invoice.AddItem("Starter Hosting", 25m, 1);
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        order.LinkInvoice(invoice.Id);
        await db.SaveChangesAsync();

        return (order.Id, order.PaymentToken, invoice.Id);
    }

    /// <summary>Seeds a client and a Pending order with one line, but no linked invoice at all.</summary>
    /// <param name="factory">The factory whose database the order is seeded in.</param>
    /// <returns>The order id and its payment token.</returns>
    public static async Task<(int OrderId, string PaymentToken)> SeedUninvoicedOrderAsync(IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var order = Order.Create($"ORD-{Guid.NewGuid():N}"[..12], client.Id, "bank-gateway", ipAddress: null);
        order.AddItem(1, "Starter Hosting", BillingCycleNames.Monthly, 25m, 25m, domain: null, hostname: null);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return (order.Id, order.PaymentToken);
    }

    /// <summary>Switches the bank-transfer integration on through the real admin endpoint.</summary>
    /// <param name="factory">The factory to mint an admin token from.</param>
    /// <returns>A task that completes once the switch is on.</returns>
    public static async Task EnableBankTransferAsync(IntegrationTestFactory factory)
    {
        var admin = await AdminClientAsync(factory);
        var response = await admin.PutAsJsonAsync(
            "/api/admin/integrations/bank-transfer",
            new { isEnabled = true, config = new Dictionary<string, string>() });
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    /// <summary>Asserts the refusal carries the expected error code, which is what a client branches on.</summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    public static async Task AssertCodeAsync(HttpResponseMessage response, string expected)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(expected);
    }
}
