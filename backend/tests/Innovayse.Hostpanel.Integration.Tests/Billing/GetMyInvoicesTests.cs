using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/me/invoices</c> against Postgres: a client lists their own invoices.
/// </summary>
/// <remarks>Split from the deleted <c>BillingEndpointTests</c> and <c>MyInvoicesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyInvoicesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Listing invoices is not for anonymous callers.</summary>
    [Fact]
    public async Task GetMyInvoicesAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/me/invoices");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>An admin, who is not a client, is refused with 403.</summary>
    [Fact]
    public async Task GetMyInvoicesAsync_Returns403_AsAdminAsync()
    {
        var client = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync("/api/me/invoices");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>A registered client lists their own (empty) invoices.</summary>
    [Fact]
    public async Task GetMyInvoicesAsync_Returns200_AsClientAsync()
    {
        var (client, _) = await BillingTestSeeds.RegisterClientAsync(factory, null, "list-mine");

        var response = await client.GetAsync("/api/me/invoices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The owner reads their own invoice list and finds the invoice they were charged.</summary>
    [Fact]
    public async Task GetMine_ListsTheOwnersInvoice()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-list");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);

        var response = await owner.GetAsync("/api/me/invoices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.EnumerateArray().Select(i => i.GetProperty("id").GetInt32()).Should().Contain(invoiceId);
    }

    /// <summary>An invoice still in draft is being prepared by staff, so the client's own list leaves it out.</summary>
    [Fact]
    public async Task GetMine_HidesAnInvoiceStillInDraft()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-draft");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var issuedId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);
        var draftId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 20m, isDraft: true);

        var response = await owner.GetAsync("/api/me/invoices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = body.EnumerateArray().Select(i => i.GetProperty("id").GetInt32()).ToList();
        ids.Should().Contain(issuedId).And.NotContain(draftId);
    }
}
