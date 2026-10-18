using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Integration tests for <c>POST /api/provisioning/{id}/suspend</c>.
/// </summary>
/// <remarks>Moved from the deleted <c>ProvisioningEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SuspendHostedAccountTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Admin can suspend a provisioned service.</summary>
    [Fact]
    public async Task Suspend_AdminCanSuspendAsync()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);
        (await http.PostAsync($"/api/provisioning/{serviceId}/provision", null)).EnsureSuccessStatusCode();

        var response = await http.PostAsJsonAsync(
            $"/api/provisioning/{serviceId}/suspend", new { reason = "Non-payment" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Suspending a service never provisioned answers 400 with <c>SERVICE_NOT_PROVISIONED</c>.</summary>
    [Fact]
    public async Task Suspend_ServiceNeverProvisioned_Returns400WithServiceNotProvisioned()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);

        var response = await http.PostAsJsonAsync(
            $"/api/provisioning/{serviceId}/suspend", new { reason = "Non-payment" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_PROVISIONED);
    }

    /// <summary>Suspending an id no service takes answers 404 with <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Suspend_UnknownService_Returns404WithServiceNotFound()
    {
        var http = factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await factory.GetAdminTokenAsync());

        var response = await http.PostAsJsonAsync(
            $"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/suspend", new { reason = "Non-payment" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>
    /// A provider that refuses to suspend answers 502 with <c>SERVICE_SUSPEND_UPSTREAM_FAILED</c>.
    /// </summary>
    [Fact]
    public async Task Suspend_ProviderRefuses_Returns502WithServiceSuspendUpstreamFailed()
    {
        var setupHttp = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var serviceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, setupHttp, adminToken);
        (await setupHttp.PostAsync($"/api/provisioning/{serviceId}/provision", null)).EnsureSuccessStatusCode();

        var failing = new FakeProvisioningProvider { ShouldFail = true };
        var http = await ProvisioningTestSeeds.AdminClientWithFakeProviderAsync(factory, failing);

        var response = await http.PostAsJsonAsync(
            $"/api/provisioning/{serviceId}/suspend", new { reason = "Non-payment" });

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_SUSPEND_UPSTREAM_FAILED);
    }

    /// <summary>Suspending without authentication is refused.</summary>
    [Fact]
    public async Task Suspend_Returns401_WithoutAuthAsync()
    {
        var http = factory.CreateClient();

        var response = await http.PostAsJsonAsync(
            $"/api/provisioning/{ProvisioningTestSeeds.NoSuchServiceId}/suspend", new { reason = "Non-payment" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
