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
/// Integration tests for the admin <c>POST /api/services/{id}/unsuspend</c> action.
/// </summary>
/// <remarks>Moved from the deleted <c>ServiceLifecycleEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UnsuspendServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Unsuspending a service that does not exist answers 404 <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Unsuspend_OnAnUnknownService_Returns404()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/unsuspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>An admin can unsuspend a suspended service.</summary>
    [Fact]
    public async Task Unsuspend_OnASuspendedService_Returns204()
    {
        var (_, ownerClientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "unsuspend-owner");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, ownerClientId);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var service = await context.ClientServices.FirstOrDefaultAsync(
                s => s.Id == serviceId, CancellationToken.None);
            service!.Activate("panel-ref-unsuspend");
            service.Suspend("test");
            await uow.SaveChangesAsync(CancellationToken.None);
        }

        var admin = await ServicesTestSeeds.AdminClientAsync(factory);
        var response = await admin.PostAsync($"/api/services/{serviceId}/unsuspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Unsuspending without authentication is refused.</summary>
    [Fact]
    public async Task Unsuspend_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/unsuspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
