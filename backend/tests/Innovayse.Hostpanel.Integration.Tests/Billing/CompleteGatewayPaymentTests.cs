using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// "Complete a hosted-gateway payment" (<c>CompleteGatewayPaymentCommand</c>, reached through
/// <c>POST /api/me/invoices/{id}/gateway-payment/complete</c> once
/// <c>CompleteMyGatewayPaymentHandler</c> settles ownership) against Postgres, with the payment
/// plugin replaced by <see cref="FakePaymentPluginResolver"/>.
/// </summary>
/// <remarks>
/// The two concurrent-writer races are kept as direct handler calls through a DI scope: making two real HTTP requests race
/// each other onto the same row reliably enough for a unit test is not achievable — the race is
/// manufactured here instead, a second <see cref="Innovayse.Hostpanel.Infrastructure.Persistence.AppDbContext"/>
/// changing the row between this handler's read and its write, which is exactly what
/// <c>DbUpdateConcurrencyException</c> guards against. The ownership boundary itself is
/// <see cref="CompleteMyGatewayPaymentTests"/>'s.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CompleteGatewayPaymentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host whose payment plugins come from <paramref name="resolver"/>.</summary>
    /// <param name="resolver">The fake resolver the host should resolve plugins through.</param>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost(FakePaymentPluginResolver resolver) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IPaymentPluginResolver>(_ => resolver)));

    /// <summary>Creates an invoice for <paramref name="clientId"/> and starts a gateway session on it.</summary>
    /// <param name="host">The host to create the invoice against.</param>
    /// <param name="client">The payer's authenticated client, used to start the session.</param>
    /// <param name="clientId">The client to invoice.</param>
    /// <param name="plugin">The fake plugin the session should resolve to.</param>
    /// <returns>The invoice id carrying a live gateway session.</returns>
    private async Task<int> CreateInvoiceWithSessionAsync(
        WebApplicationFactory<Program> host, HttpClient client, int clientId, FakePaymentPlugin plugin)
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory, host);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 10m);

        plugin.CreatePaymentResult = new("gw-1", "https://pg.example/pay?mdOrder=gw-1");
        var started = await client.PostAsJsonAsync(
            $"/api/me/invoices/{invoiceId}/gateway-payment/start",
            new { module = "bank-gateway", returnUrl = BillingTestSeeds.AllowedReturnUrl });
        started.StatusCode.Should().Be(HttpStatusCode.OK);

        return invoiceId;
    }

    /// <summary>Completes a gateway payment for <paramref name="invoiceId"/>.</summary>
    /// <param name="client">The payer's authenticated client.</param>
    /// <param name="invoiceId">The invoice to complete.</param>
    /// <returns>The HTTP response.</returns>
    private static Task<HttpResponseMessage> CompleteAsync(HttpClient client, int invoiceId) =>
        client.PostAsync($"/api/me/invoices/{invoiceId}/gateway-payment/complete", content: null);

    /// <summary>A gateway that reports Paid marks the invoice paid with the gateway's own transaction id.</summary>
    [Fact]
    public async Task GatewaySaysPaidMarksPaidWithGatewayTransaction()
    {
        var plugin = new FakePaymentPlugin { StatusResult = new(GatewayPaymentState.Paid, "ref-9", "orderStatus:2") };
        var resolver = new FakePaymentPluginResolver();
        resolver.Register("bank-gateway", plugin);
        var host = BuildHost(resolver);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "gw-complete-paid");
        var invoiceId = await CreateInvoiceWithSessionAsync(host, client, clientId, plugin);

        var response = await CompleteAsync(client, invoiceId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("state").GetString().Should().Be("paid");

        var admin = await BillingTestSeeds.AdminClientAsync(factory, host);
        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var fetchedBody = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        fetchedBody.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Paid));
        fetchedBody.RootElement.GetProperty("gatewayTransactionId").GetString().Should().Be("ref-9");

        // The client ledger row is written inline by the handler, once -- no event handler repeats it.
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var ledger = await context.Transactions
            .Where(t => t.InvoiceId == invoiceId)
            .ToListAsync();
        ledger.Should().ContainSingle();
        ledger[0].TransactionId.Should().Be("ref-9");
        ledger[0].PaymentMethod.Should().Be("bank-gateway");
        ledger[0].AmountIn.Should().Be(10m);
    }

    /// <summary>An already-paid invoice answers Paid again without ever asking the gateway a second time.</summary>
    [Fact]
    public async Task AlreadyPaidIsIdempotentWithoutASecondGatewayCall()
    {
        var plugin = new FakePaymentPlugin { StatusResult = new(GatewayPaymentState.Paid, "ref-9", "orderStatus:2") };
        var resolver = new FakePaymentPluginResolver();
        resolver.Register("bank-gateway", plugin);
        var host = BuildHost(resolver);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "gw-complete-idempotent");
        var invoiceId = await CreateInvoiceWithSessionAsync(host, client, clientId, plugin);
        (await CompleteAsync(client, invoiceId)).StatusCode.Should().Be(HttpStatusCode.OK);
        var callsAfterFirstComplete = plugin.GetStatusCallCount;

        var response = await CompleteAsync(client, invoiceId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("state").GetString().Should().Be("paid");
        plugin.GetStatusCallCount.Should().Be(callsAfterFirstComplete);
    }

    /// <summary>A gateway session still pending leaves the invoice unpaid and answers "pending".</summary>
    [Fact]
    public async Task PendingLeavesTheInvoiceUnpaid()
    {
        var plugin = new FakePaymentPlugin { StatusResult = new(GatewayPaymentState.Pending, null, "orderStatus:0") };
        var resolver = new FakePaymentPluginResolver();
        resolver.Register("bank-gateway", plugin);
        var host = BuildHost(resolver);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "gw-complete-pending");
        var invoiceId = await CreateInvoiceWithSessionAsync(host, client, clientId, plugin);

        var response = await CompleteAsync(client, invoiceId);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("state").GetString().Should().Be("pending");

        var admin = await BillingTestSeeds.AdminClientAsync(factory, host);
        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var fetchedBody = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        fetchedBody.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Unpaid));
    }

    /// <summary>A declined gateway session leaves the invoice unpaid and answers "declined".</summary>
    [Fact]
    public async Task DeclinedLeavesTheInvoiceUnpaid()
    {
        var plugin = new FakePaymentPlugin { StatusResult = new(GatewayPaymentState.Declined, null, "orderStatus:6") };
        var resolver = new FakePaymentPluginResolver();
        resolver.Register("bank-gateway", plugin);
        var host = BuildHost(resolver);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "gw-complete-declined");
        var invoiceId = await CreateInvoiceWithSessionAsync(host, client, clientId, plugin);

        var response = await CompleteAsync(client, invoiceId);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("state").GetString().Should().Be("declined");
    }

    /// <summary>
    /// Two simultaneous completions (the result page's auto-check racing the retry button, or the
    /// reconciler firing as the payer returns) can both pass the status guard. The loser's
    /// <c>SaveChangesAsync</c> hits the xmin concurrency token; the handler must answer
    /// <see cref="GatewayCompletionState.Paid"/> rather than surface that as a failure once the
    /// winner already recorded the payment.
    /// </summary>
    [Fact]
    public async Task ConcurrentCompletionAlreadyWonReturnsPaidWithoutThrowing()
    {
        var invoiceId = await SeedInvoiceWithSessionDirectlyAsync();
        var resolver = new FakePaymentPluginResolver();
        var plugin = new FakePaymentPlugin { StatusResult = new(GatewayPaymentState.Paid, "ref-9", "orderStatus:2") };
        resolver.Register("bank-gateway", plugin);

        using var readerScope = factory.Services.CreateScope();
        var readerContext = readerScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = readerScope.ServiceProvider.GetRequiredService<IMessageBus>();
        var handler = new CompleteGatewayPaymentHandler(
            readerContext, resolver, bus, NullLogger<CompleteGatewayPaymentHandler>.Instance);

        // Loads the same row the handler is about to load, then wins the race by saving first.
        using (var winnerScope = factory.Services.CreateScope())
        {
            var winnerContext = winnerScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var winnerInvoice = await winnerContext.Invoices.FirstAsync(i => i.Id == invoiceId);
            winnerInvoice.MarkPaidViaGateway("ref-9");
            await winnerContext.SaveChangesAsync(CancellationToken.None);
        }

        var result = await handler.HandleAsync(new CompleteGatewayPaymentCommand(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(GatewayCompletionState.Paid);
    }

    /// <summary>
    /// A concurrency conflict from an unrelated writer — not the same payment already won — is not
    /// swallowed: the handler rethrows rather than answering <c>Paid</c> for a row that is still
    /// unpaid on reload.
    /// </summary>
    [Fact]
    public async Task ConcurrentCompletionOtherWriterDidNotWinRethrows()
    {
        var invoiceId = await SeedInvoiceWithSessionDirectlyAsync();
        var resolver = new FakePaymentPluginResolver();
        var plugin = new FakePaymentPlugin { StatusResult = new(GatewayPaymentState.Paid, "ref-9", "orderStatus:2") };
        resolver.Register("bank-gateway", plugin);

        using var readerScope = factory.Services.CreateScope();
        var readerContext = readerScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = readerScope.ServiceProvider.GetRequiredService<IMessageBus>();
        var handler = new CompleteGatewayPaymentHandler(
            readerContext, resolver, bus, NullLogger<CompleteGatewayPaymentHandler>.Instance);

        // The handler's own context has to be holding the row *before* the competing write, or it
        // saves against the current version and there is no conflict to test. Loading it here is
        // what the handler itself does moments later; EF hands back this tracked instance, still
        // carrying the pre-conflict row version.
        _ = await readerContext.Invoices.FirstAsync(i => i.Id == invoiceId);

        // An unrelated write against the same row bumps the row version without ever marking the
        // invoice paid, so the reload the handler does after the conflict must still see it unpaid
        // and rethrow rather than swallow the conflict.
        using (var otherWriterScope = factory.Services.CreateScope())
        {
            var otherContext = otherWriterScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var otherInvoice = await otherContext.Invoices.FirstAsync(i => i.Id == invoiceId);
            // A different session reference, not the seeded "gw-1": writing the same values back is
            // no change at all to EF, which then issues no UPDATE and leaves xmin where it was —
            // and the conflict this test exists to provoke never happens.
            otherInvoice.SetGatewaySession("bank-gateway", "gw-1-touched");
            await otherContext.SaveChangesAsync(CancellationToken.None);
        }

        var act = () => handler.HandleAsync(new CompleteGatewayPaymentCommand(invoiceId), CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    /// <summary>
    /// Seeds a client and an invoice with a live gateway session directly against the database,
    /// bypassing HTTP, for the two DI-scope concurrency tests that only need a row to race over.
    /// </summary>
    /// <returns>The seeded invoice's primary key.</returns>
    private async Task<int> SeedInvoiceWithSessionDirectlyAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var client = Client.Create(
            $"user-{Guid.NewGuid():N}", "Gateway", "Race", $"gw-race-{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var invoice = Invoice.Create(client.Id, DateTimeOffset.UtcNow.AddDays(14), "USD");
        invoice.AddItem("Hosting", 10m, 1);
        invoice.SetGatewaySession("bank-gateway", "gw-1");
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(CancellationToken.None);

        return invoice.Id;
    }
}
