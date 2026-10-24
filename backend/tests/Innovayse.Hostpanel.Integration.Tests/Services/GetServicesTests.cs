using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>GET /api/services</c>, the admin paged list of client services.
/// </summary>
/// <remarks>Moved from the deleted <c>ServicesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetServicesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET /api/services without auth returns 401 Unauthorized.</summary>
    [Fact]
    public async Task GetServicesAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client calling GET /api/services is refused -- the policy is reseller or admin.</summary>
    [Fact]
    public async Task GetServicesAsync_Returns403_AsPlainClientAsync()
    {
        var (client, _) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "get-services-client");

        var response = await client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Admin GET /api/services returns 200 OK with paged results.</summary>
    [Fact]
    public async Task GetServicesAsync_Returns200_AsAdminAsync()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A service that runs on a site of its own, not on a registered domain, shows that site in the admin list instead of an empty cell.</summary>
    [Fact]
    public async Task GetServicesAsync_ShowsTheServicesOwnDomainWhenNoRegisteredDomainIsLinkedAsync()
    {
        var admin = await ServicesTestSeeds.AdminClientAsync(factory);
        var domain = $"site-{Guid.NewGuid():N}.example";
        int serviceId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 5m);
            var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
            service.Update(
                domain: domain, dedicatedIp: null, username: null, password: null, billingCycle: BillingCycleNames.Monthly,
                recurringAmount: 5m, paymentMethod: null, nextRenewalAt: null, subscriptionId: null, overrideAutoSuspend: false,
                suspendUntil: null, autoTerminateEndOfCycle: false, autoTerminateReason: null, adminNotes: null,
                provisioningRef: null, firstPaymentAmount: 5m, promotionCode: null, terminatedAt: null, serverId: null,
                quantity: 1, productId: null);
            context.ClientServices.Add(service);
            await context.SaveChangesAsync(CancellationToken.None);
            serviceId = service.Id;
        }

        var response = await admin.GetAsync("/api/services?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = body.RootElement.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("id").GetInt32() == serviceId);
        row.GetProperty("domain").GetString().Should().Be(domain);
    }
}
