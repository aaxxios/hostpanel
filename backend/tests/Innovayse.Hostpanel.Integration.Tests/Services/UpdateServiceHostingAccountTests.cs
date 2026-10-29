using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceHostingAccount;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>PUT /api/services/{id}/hosting-account</c> and <see cref="UpdateServiceHostingAccountHandler"/>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateServiceHostingAccountTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A valid save answers 204 and stores the account details on a service in any status.</summary>
    [Fact]
    public async Task UpdateHostingAccount_OnAnActiveService_StoresTheDetailsAsync()
    {
        var serviceId = await ServicesTestSeeds.SeedServiceAsync(factory, ServiceStatus.Active);
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/services/{serviceId}/hosting-account", new
        {
            domain = "site.example",
            provisioningRef = "acct2",
            dedicatedIp = "198.51.100.7",
            username = "siteuser",
            password = "s3cret-pass",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var stored = await context.ClientServices.AsNoTracking().SingleAsync(s => s.Id == serviceId);
        stored.ServerId.Should().BeNull();
        stored.Domain.Should().Be("site.example");
        stored.ProvisioningRef.Should().Be("acct2");
        stored.DedicatedIp.Should().Be("198.51.100.7");
        stored.Username.Should().Be("siteuser");
        stored.Password.Should().Be("s3cret-pass");
        stored.Status.Should().Be(ServiceStatus.Active);
    }

    /// <summary>A domain longer than a domain can be is refused with 400 <c>VALIDATION_ERROR</c>.</summary>
    [Fact]
    public async Task UpdateHostingAccount_WithATooLongDomain_Returns400ValidationErrorAsync()
    {
        var serviceId = await ServicesTestSeeds.SeedServiceAsync(factory, ServiceStatus.Active);
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/services/{serviceId}/hosting-account", new { domain = new string('a', 254) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }

    /// <summary>Saving the account of a service that does not exist answers 404 <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateHostingAccount_OnAnUnknownService_Returns404Async()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/services/{ServicesTestSeeds.NoSuchServiceId}/hosting-account", new { domain = "site.example" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>A server id that names no server is refused with 404 <c>SERVER_NOT_FOUND</c> and nothing is stored.</summary>
    [Fact]
    public async Task UpdateHostingAccount_WithAnUnknownServer_Returns404AndKeepsTheServiceAsync()
    {
        var serviceId = await ServicesTestSeeds.SeedServiceAsync(factory, ServiceStatus.Active);
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/services/{serviceId}/hosting-account", new { serverId = 999_999_999, domain = "site.example" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVER_NOT_FOUND);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var stored = await context.ClientServices.AsNoTracking().SingleAsync(s => s.Id == serviceId);
        stored.ServerId.Should().BeNull();
        stored.Domain.Should().NotBe("site.example");
    }

    /// <summary>An existing server is accepted and stored on the service.</summary>
    [Fact]
    public async Task UpdateHostingAccount_WithAnExistingServer_StoresTheServerAsync()
    {
        var serviceId = await ServicesTestSeeds.SeedServiceAsync(factory, ServiceStatus.Active);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "hosting-account.example.test", 22);
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/services/{serviceId}/hosting-account", new { serverId, domain = "site.example" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var stored = await context.ClientServices.AsNoTracking().SingleAsync(s => s.Id == serviceId);
        stored.ServerId.Should().Be(serverId);
    }

    /// <summary>Saving without authentication is refused.</summary>
    [Fact]
    public async Task UpdateHostingAccount_WithoutAuth_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/services/{ServicesTestSeeds.NoSuchServiceId}/hosting-account", new { domain = "site.example" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
