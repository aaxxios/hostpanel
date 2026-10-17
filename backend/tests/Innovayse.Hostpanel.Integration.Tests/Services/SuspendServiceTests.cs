using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for the admin <c>POST /api/services/{id}/suspend</c> action.
/// </summary>
/// <remarks>
/// Moved from the deleted <c>ServiceLifecycleEndpointTests</c> and
/// <c>MyServicesOwnershipEndpointTests</c>.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SuspendServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Suspending a service that does not exist answers 404 <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Suspend_OnAnUnknownService_Returns404()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/suspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>An admin may suspend any client's service -- the admin surface is not ownership-scoped.</summary>
    [Fact]
    public async Task AdminSuspend_AnyClientsService_StillSucceeds()
    {
        var (_, ownerClientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "suspend-admin-owner");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, ownerClientId);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var service = await context.ClientServices.FirstOrDefaultAsync(
                s => s.Id == serviceId, CancellationToken.None);
            service!.Activate("panel-ref-admin-test");
            await uow.SaveChangesAsync(CancellationToken.None);
        }

        var admin = await ServicesTestSeeds.AdminClientAsync(factory);
        var response = await admin.PostAsync($"/api/services/{serviceId}/suspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Suspending without authentication is refused.</summary>
    [Fact]
    public async Task Suspend_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/suspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client calling the admin suspend route is refused -- the policy is admin only.</summary>
    [Fact]
    public async Task Suspend_Returns403_AsPlainClientAsync()
    {
        var (client, _) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "suspend-forbidden");

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/suspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
