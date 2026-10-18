using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.SDK.Plugins;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <see cref="ReconcileGatewayPaymentsCronHandler"/> against Postgres: which unpaid/overdue
/// invoices its 25-minute-to-24-hour gateway-session window picks up, now that the query is
/// inlined against <see cref="IHostpanelDbContext"/> instead of the deleted
/// <c>IInvoiceRepository.ListPendingGatewayPaymentsAsync</c> (A6 Task 4, ruling 2 -- this pins
/// that dissolved method's semantics).
/// </summary>
/// <remarks>
/// <para>
/// A picked-up invoice is observed indirectly: the reconciler dispatches
/// <c>CompleteGatewayPaymentCommand</c> for each match, which calls
/// <see cref="IPaymentPlugin.GetStatusAsync"/> exactly once per invoice it actually reconciles
/// (an invoice already Paid or with no session short-circuits before that call, but every
/// invoice this suite seeds is Unpaid/Overdue with a session, so
/// <see cref="FakePaymentPlugin.GetStatusCallCount"/> counts precisely which invoices the
/// window's filter selected).
/// </para>
/// <para>
/// <see cref="IntegrationTestFactory"/> is a class fixture, so the same Postgres database is
/// shared by every test method here; the reconciler's query is not scoped to one client, so a
/// module id shared across tests would let one test's leftover invoice be resolved -- and
/// counted -- against a later test's plugin. Each test therefore mints its own module id, so an
/// older test's invoice (still in the window when a later test runs moments later) resolves to
/// no plugin at all for that later test and is silently skipped, exactly as an unconfigured
/// module id is in production.
/// </para>
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ReconcileGatewayPaymentsCronTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host whose payment plugins come from <paramref name="resolver"/>.</summary>
    /// <param name="resolver">The fake resolver the host should resolve plugins through.</param>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost(FakePaymentPluginResolver resolver) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IPaymentPluginResolver>(_ => resolver)));

    /// <summary>
    /// Seeds a client and one invoice carrying a gateway session under <paramref name="module"/>,
    /// with <see cref="Invoice.GatewayStartedAt"/> backdated directly at the database, the way
    /// <see cref="Invoice.SetGatewaySession"/>'s <c>DateTimeOffset.UtcNow</c> stamp cannot be.
    /// </summary>
    /// <param name="db">The data context to seed through.</param>
    /// <param name="module">This test's own plugin module id.</param>
    /// <param name="status">The invoice's status; <see cref="InvoiceStatus.Unpaid"/> or <see cref="InvoiceStatus.Overdue"/>.</param>
    /// <param name="gatewayStartedAt">The backdated session start instant.</param>
    /// <returns>The seeded invoice's primary key.</returns>
    private static async Task<int> SeedPendingInvoiceAsync(
        AppDbContext db, string module, InvoiceStatus status, DateTimeOffset gatewayStartedAt)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Gateway", "Payer", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);
        await db.SaveChangesAsync(CancellationToken.None);

        var invoice = Invoice.Create(client.Id, DateTimeOffset.UtcNow.AddDays(7), "USD");
        if (status == InvoiceStatus.Overdue)
        {
            invoice.MarkOverdue();
        }

        invoice.SetGatewaySession(module, $"gw-{Guid.NewGuid():N}");
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(CancellationToken.None);

        db.Entry(invoice).Property(i => i.GatewayStartedAt).CurrentValue = gatewayStartedAt;
        await db.SaveChangesAsync(CancellationToken.None);

        return invoice.Id;
    }

    /// <summary>
    /// An Unpaid invoice whose gateway session started 1 hour ago (inside the 25-minute-to-24-hour
    /// window) is reconciled.
    /// </summary>
    [Fact]
    public async Task UnpaidInvoiceWithinWindow_IsReconciled()
    {
        var module = $"reconcile-test-{Guid.NewGuid():N}";
        var plugin = new FakePaymentPlugin();
        var resolver = new FakePaymentPluginResolver();
        resolver.Register(module, plugin);
        var host = BuildHost(resolver);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedPendingInvoiceAsync(db, module, InvoiceStatus.Unpaid, DateTimeOffset.UtcNow.AddHours(-1));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ReconcileGatewayPaymentsCronCommand(), CancellationToken.None);

        plugin.GetStatusCallCount.Should().Be(1);
    }

    /// <summary>
    /// An Overdue invoice within the window is reconciled too -- the filter ORs Unpaid and
    /// Overdue, it does not restrict to Unpaid alone.
    /// </summary>
    [Fact]
    public async Task OverdueInvoiceWithinWindow_IsAlsoReconciled()
    {
        var module = $"reconcile-test-{Guid.NewGuid():N}";
        var plugin = new FakePaymentPlugin();
        var resolver = new FakePaymentPluginResolver();
        resolver.Register(module, plugin);
        var host = BuildHost(resolver);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedPendingInvoiceAsync(db, module, InvoiceStatus.Overdue, DateTimeOffset.UtcNow.AddHours(-1));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ReconcileGatewayPaymentsCronCommand(), CancellationToken.None);

        plugin.GetStatusCallCount.Should().Be(1);
    }

    /// <summary>
    /// A session that started 10 minutes ago is younger than the 25-minute settle margin --
    /// the payer may still be on the payment page, so it is left alone.
    /// </summary>
    [Fact]
    public async Task SessionYoungerThanSettleMargin_IsNotReconciled()
    {
        var module = $"reconcile-test-{Guid.NewGuid():N}";
        var plugin = new FakePaymentPlugin();
        var resolver = new FakePaymentPluginResolver();
        resolver.Register(module, plugin);
        var host = BuildHost(resolver);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedPendingInvoiceAsync(db, module, InvoiceStatus.Unpaid, DateTimeOffset.UtcNow.AddMinutes(-10));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ReconcileGatewayPaymentsCronCommand(), CancellationToken.None);

        plugin.GetStatusCallCount.Should().Be(0);
    }

    /// <summary>
    /// A session that started 25 hours ago is past the 24-hour abandoned cutoff -- it is left
    /// for audit rather than checked forever.
    /// </summary>
    [Fact]
    public async Task SessionOlderThanAbandonedCutoff_IsNotReconciled()
    {
        var module = $"reconcile-test-{Guid.NewGuid():N}";
        var plugin = new FakePaymentPlugin();
        var resolver = new FakePaymentPluginResolver();
        resolver.Register(module, plugin);
        var host = BuildHost(resolver);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedPendingInvoiceAsync(db, module, InvoiceStatus.Unpaid, DateTimeOffset.UtcNow.AddHours(-25));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ReconcileGatewayPaymentsCronCommand(), CancellationToken.None);

        plugin.GetStatusCallCount.Should().Be(0);
    }

    /// <summary>An Unpaid invoice with no gateway session at all is never picked up.</summary>
    [Fact]
    public async Task InvoiceWithNoGatewaySession_IsNotReconciled()
    {
        var module = $"reconcile-test-{Guid.NewGuid():N}";
        var plugin = new FakePaymentPlugin();
        var resolver = new FakePaymentPluginResolver();
        resolver.Register(module, plugin);
        var host = BuildHost(resolver);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "No", "Session", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);
        await db.SaveChangesAsync(CancellationToken.None);
        var invoice = Invoice.Create(client.Id, DateTimeOffset.UtcNow.AddDays(7), "USD");
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ReconcileGatewayPaymentsCronCommand(), CancellationToken.None);

        plugin.GetStatusCallCount.Should().Be(0);
    }
}
