using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Integration tests for <c>POST /api/provisioning/{id}/provision</c>.
/// </summary>
/// <remarks>Moved from the deleted <c>ProvisioningEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ProvisionServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Admin can provision a pending service.</summary>
    [Fact]
    public async Task Provision_AdminCanProvisionServiceAsync()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();

        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);

        var response = await http.PostAsync($"/api/provisioning/{serviceId}/provision", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Client role calling /api/provisioning/{id}/provision receives 403 Forbidden.</summary>
    [Fact]
    public async Task Provision_ClientForbiddenAsync()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();

        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);

        var email = $"prov-forbidden-{Guid.NewGuid():N}@example.com";
        http.DefaultRequestHeaders.Authorization = null;

        var registerResponse = await http.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass@123!",
            firstName = "Forbidden",
            lastName = "Client"
        });
        registerResponse.EnsureSuccessStatusCode();

        var clientToken = await factory.GetClientTokenAsync(email, "Pass@123!");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var response = await http.PostAsync($"/api/provisioning/{serviceId}/provision", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Provisioning an id no service takes answers 404 with <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Provision_UnknownService_Returns404WithServiceNotFound()
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await factory.GetAdminTokenAsync());

        var response = await http.PostAsync($"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/provision", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>
    /// A provider that refuses to provision answers 502 with <c>PROVISIONING_UPSTREAM_FAILED</c>,
    /// and no provider detail reaches the body.
    /// </summary>
    [Fact]
    public async Task Provision_ProviderRefuses_Returns502WithProvisioningUpstreamFailed()
    {
        var setupHttp = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, setupHttp, adminToken);

        var failing = new FakeProvisioningProvider { ShouldFail = true };
        var http = await ProvisioningTestSeeds.AdminClientWithFakeProviderAsync(factory, failing);

        var response = await http.PostAsync($"/api/provisioning/{serviceId}/provision", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.PROVISIONING_UPSTREAM_FAILED);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("fake provider");
    }

    /// <summary>Provisioning without authentication is refused.</summary>
    [Fact]
    public async Task Provision_Returns401_WithoutAuthAsync()
    {
        var http = factory.CreateClient();

        var response = await http.PostAsync($"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/provision", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
