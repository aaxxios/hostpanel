using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Integration tests for <c>GET /api/provisioning/{id}/credentials</c>.
/// </summary>
/// <remarks>Moved from the deleted <c>ProvisioningEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetServiceCredentialsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Admin can retrieve credentials for a provisioned service.</summary>
    [Fact]
    public async Task GetCredentials_AdminCanGetCredentialsAsync()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();

        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);

        var provisionResponse = await http.PostAsync($"/api/provisioning/{serviceId}/provision", null);
        provisionResponse.EnsureSuccessStatusCode();

        var response = await http.GetAsync($"/api/provisioning/{serviceId}/credentials");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Reading credentials for a never-provisioned service answers 400 with <c>SERVICE_NOT_PROVISIONED</c>.</summary>
    [Fact]
    public async Task GetCredentials_ServiceNeverProvisioned_Returns400WithServiceNotProvisioned()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);

        var response = await http.GetAsync($"/api/provisioning/{serviceId}/credentials");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_PROVISIONED);
    }

    /// <summary>Reading credentials for an id no service takes answers 404 with <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task GetCredentials_UnknownService_Returns404WithServiceNotFound()
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await factory.GetAdminTokenAsync());

        var response = await http.GetAsync($"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/credentials");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>
    /// A provider that refuses to return credentials answers 502 with
    /// <c>SERVICE_CREDENTIALS_UPSTREAM_FAILED</c>.
    /// </summary>
    [Fact]
    public async Task GetCredentials_ProviderRefuses_Returns502WithServiceCredentialsUpstreamFailed()
    {
        var setupHttp = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, setupHttp, adminToken);
        (await setupHttp.PostAsync($"/api/provisioning/{serviceId}/provision", null)).EnsureSuccessStatusCode();

        var failing = new FakeProvisioningProvider { ShouldFail = true };
        var http = await ProvisioningTestSeeds.AdminClientWithFakeProviderAsync(factory, failing);

        var response = await http.GetAsync($"/api/provisioning/{serviceId}/credentials");

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_CREDENTIALS_UPSTREAM_FAILED);
    }

    /// <summary>Reading credentials without authentication is refused.</summary>
    [Fact]
    public async Task GetCredentials_Returns401_WithoutAuthAsync()
    {
        var http = factory.CreateClient();

        var response = await http.GetAsync($"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/credentials");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
