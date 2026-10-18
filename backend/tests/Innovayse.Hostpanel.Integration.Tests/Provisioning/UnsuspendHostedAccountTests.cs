using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Integration tests for <c>POST /api/provisioning/{id}/unsuspend</c>.
/// </summary>
/// <remarks>Moved from the deleted <c>ProvisioningEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UnsuspendHostedAccountTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Admin can unsuspend a suspended service.</summary>
    [Fact]
    public async Task Unsuspend_AdminCanUnsuspendAsync()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);
        (await http.PostAsync($"/api/provisioning/{serviceId}/provision", null)).EnsureSuccessStatusCode();
        (await http.PostAsJsonAsync($"/api/provisioning/{serviceId}/suspend", new { reason = "Non-payment" }))
            .EnsureSuccessStatusCode();

        var response = await http.PostAsync($"/api/provisioning/{serviceId}/unsuspend", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Unsuspending a service never provisioned answers 400 with <c>SERVICE_NOT_PROVISIONED</c>.</summary>
    [Fact]
    public async Task Unsuspend_ServiceNeverProvisioned_Returns400WithServiceNotProvisioned()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);

        var response = await http.PostAsync($"/api/provisioning/{serviceId}/unsuspend", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_PROVISIONED);
    }

    /// <summary>Unsuspending an id no service takes answers 404 with <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Unsuspend_UnknownService_Returns404WithServiceNotFound()
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await factory.GetAdminTokenAsync());

        var response = await http.PostAsync($"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/unsuspend", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>
    /// A provider that refuses to unsuspend answers 502 with
    /// <c>SERVICE_UNSUSPEND_UPSTREAM_FAILED</c>.
    /// </summary>
    [Fact]
    public async Task Unsuspend_ProviderRefuses_Returns502WithServiceUnsuspendUpstreamFailed()
    {
        var setupHttp = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, setupHttp, adminToken);
        (await setupHttp.PostAsync($"/api/provisioning/{serviceId}/provision", null)).EnsureSuccessStatusCode();
        (await setupHttp.PostAsJsonAsync($"/api/provisioning/{serviceId}/suspend", new { reason = "Non-payment" }))
            .EnsureSuccessStatusCode();

        var failing = new FakeProvisioningProvider { ShouldFail = true };
        var http = await ProvisioningTestSeeds.AdminClientWithFakeProviderAsync(factory, failing);

        var response = await http.PostAsync($"/api/provisioning/{serviceId}/unsuspend", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_UNSUSPEND_UPSTREAM_FAILED);
    }

    /// <summary>Unsuspending without authentication is refused.</summary>
    [Fact]
    public async Task Unsuspend_Returns401_WithoutAuthAsync()
    {
        var http = factory.CreateClient();

        var response = await http.PostAsync($"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/unsuspend", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
