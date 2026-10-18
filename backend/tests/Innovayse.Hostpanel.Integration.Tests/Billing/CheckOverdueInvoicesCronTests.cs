using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.CheckOverdueInvoicesCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <see cref="CheckOverdueInvoicesCronHandler"/> against Postgres: which unpaid invoices its
/// <c>DueDate &lt; now</c> filter transitions to <see cref="InvoiceStatus.Overdue"/>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CheckOverdueInvoicesCronTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Back-dates an invoice's due date through the real "update options" endpoint — there is no
    /// direct-write shortcut for this, since the cron's own filter, not a seeded row, is what
    /// every case here proves.
    /// </summary>
    /// <param name="admin">The authenticated admin client.</param>
    /// <param name="invoiceId">The invoice to back-date.</param>
    /// <param name="dueDate">The due date to write.</param>
    private static async Task BackDateAsync(HttpClient admin, int invoiceId, DateTimeOffset dueDate)
    {
        var response = await admin.PutAsJsonAsync($"/api/billing/{invoiceId}/options", new
        {
            invoiceDate = DateTimeOffset.UtcNow.AddDays(-30),
            dueDate,
            paymentMethod = (string?)null,
            taxRate = 0m,
        });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>An unpaid invoice whose due date has already passed is marked overdue.</summary>
    [Fact]
    public async Task UnpaidInvoicePastDueDate_IsMarkedOverdue()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m);
        await BackDateAsync(admin, invoiceId, DateTimeOffset.UtcNow.AddDays(-1));

        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new CheckOverdueInvoicesCronCommand(), CancellationToken.None);

        using var readScope = factory.Services.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var reloaded = await readContext.Invoices.AsNoTracking()
            .FirstAsync(i => i.Id == invoiceId, CancellationToken.None);
        reloaded.Status.Should().Be(InvoiceStatus.Overdue);
    }

    /// <summary>An unpaid invoice not yet past its due date is left untouched.</summary>
    [Fact]
    public async Task UnpaidInvoiceNotYetDue_IsNotMarkedOverdue()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m);

        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new CheckOverdueInvoicesCronCommand(), CancellationToken.None);

        using var readScope = factory.Services.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var reloaded = await readContext.Invoices.AsNoTracking()
            .FirstAsync(i => i.Id == invoiceId, CancellationToken.None);
        reloaded.Status.Should().Be(InvoiceStatus.Unpaid);
    }

    /// <summary>A draft invoice past what would be its due date is never touched — only Unpaid counts.</summary>
    [Fact]
    public async Task DraftInvoice_IsNeverMarkedOverdue()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m, isDraft: true);
        await BackDateAsync(admin, invoiceId, DateTimeOffset.UtcNow.AddDays(-1));

        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new CheckOverdueInvoicesCronCommand(), CancellationToken.None);

        using var readScope = factory.Services.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var reloaded = await readContext.Invoices.AsNoTracking()
            .FirstAsync(i => i.Id == invoiceId, CancellationToken.None);
        reloaded.Status.Should().Be(InvoiceStatus.Draft);
    }
}
