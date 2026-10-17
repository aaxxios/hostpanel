using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.ProcessBillableItemsCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <see cref="ProcessBillableItemsCronHandler"/> against Postgres: which due billable items,
/// one-time and recurring, are invoiced and how a recurring item is advanced afterwards.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ProcessBillableItemsCronTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client and one billable item due now, directly against the database.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <param name="type">Whether the item is one-time or recurring.</param>
    /// <param name="nextDueDate">The item's recorded due date.</param>
    /// <returns>The client id and the seeded item.</returns>
    private static async Task<(int ClientId, BillableItem Item)> SeedDueItemAsync(
        IHostpanelDbContext context, BillableItemType type, DateTimeOffset? nextDueDate)
    {
        var client = Client.Create(
            $"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var item = BillableItem.Create(
            client.Id, "Consulting", 40m, "USD", type,
            recurringPeriod: type == BillableItemType.Recurring ? BillingCycleNames.Monthly : null, nextDueDate: nextDueDate);
        context.BillableItems.Add(item);
        await context.SaveChangesAsync(CancellationToken.None);

        return (client.Id, item);
    }

    /// <summary>A one-time item due now is invoiced and marked invoiced.</summary>
    [Fact]
    public async Task DueOneTimeItem_IsInvoiced()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, item) = await SeedDueItemAsync(
            context, BillableItemType.OneTime, DateTimeOffset.UtcNow.AddDays(-1));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessBillableItemsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().NotBeNull();
        invoice!.Items.Should().ContainSingle(i => i.Description == "Consulting" && i.UnitPrice == 40m);

        var reloaded = await context.BillableItems.AsNoTracking()
            .FirstAsync(x => x.Id == item.Id, CancellationToken.None);
        reloaded.IsInvoiced.Should().BeTrue();
        reloaded.InvoiceId.Should().Be(invoice.Id);
    }

    /// <summary>A one-time item not yet due is left uninvoiced.</summary>
    [Fact]
    public async Task NotYetDueOneTimeItem_IsNotInvoiced()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, item) = await SeedDueItemAsync(
            context, BillableItemType.OneTime, DateTimeOffset.UtcNow.AddDays(5));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessBillableItemsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().BeNull();

        var reloaded = await context.BillableItems.AsNoTracking()
            .FirstAsync(x => x.Id == item.Id, CancellationToken.None);
        reloaded.IsInvoiced.Should().BeFalse();
    }

    /// <summary>A recurring item due now is invoiced and re-armed: its due date advances and it is eligible again.</summary>
    [Fact]
    public async Task DueRecurringItem_IsInvoicedAndAdvanced()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var due = DateTimeOffset.UtcNow.AddDays(-1);
        var (clientId, item) = await SeedDueItemAsync(context, BillableItemType.Recurring, due);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessBillableItemsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().NotBeNull();
        invoice!.Items.Should().ContainSingle(i => i.Description == "Consulting" && i.UnitPrice == 40m);

        var reloaded = await context.BillableItems.AsNoTracking()
            .FirstAsync(x => x.Id == item.Id, CancellationToken.None);
        // AdvanceDueDate re-arms a recurring item for its next cycle, so it reads as not yet invoiced.
        reloaded.IsInvoiced.Should().BeFalse();
        reloaded.InvoiceId.Should().BeNull();
        reloaded.NextDueDate.Should().BeAfter(due);
    }
}
