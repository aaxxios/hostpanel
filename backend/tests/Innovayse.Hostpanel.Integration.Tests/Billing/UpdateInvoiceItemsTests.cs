using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>PUT /api/billing/{id}/items</c> against Postgres: adding, editing and removing line items
/// on an invoice, and the <c>INVOICE_NOT_FOUND</c> refusal for an id that names no invoice.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateInvoiceItemsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Replacing an invoice's single line item rewrites its total.</summary>
    [Fact]
    public async Task UpdateItemsRewritesTheTotal()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 10m);
        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var fetchedBody = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        var itemId = fetchedBody.RootElement.GetProperty("items")[0].GetProperty("id").GetInt32();

        var response = await admin.PutAsJsonAsync($"/api/billing/{invoiceId}/items", new
        {
            items = new[] { new { id = itemId, description = "Hosting", unitPrice = 25m, quantity = 1, isDeleted = false } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reFetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var reFetchedBody = JsonDocument.Parse(await reFetched.Content.ReadAsStringAsync());
        reFetchedBody.RootElement.GetProperty("subTotal").GetDecimal().Should().Be(25m);
    }

    /// <summary>Updating line items on an id that names no invoice answers 404, not 400.</summary>
    [Fact]
    public async Task UpdateItemsOnUnknownInvoiceRefusesNotFound()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/items", new
        {
            items = new[] { new { id = (int?)null, description = "Line", unitPrice = 1m, quantity = 1, isDeleted = false } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Updating line items is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateItemsRefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/billing/1/items", new
        {
            items = Array.Empty<object>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
