using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>GetTldConfig</c> use case —
/// <c>GET /api/admin/tld-configs/{id}</c>. The happy path (create then read) lives in
/// <see cref="CreateTldConfigTests"/>; this file covers the id refusal.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetTldConfigTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Reading an id that names no configuration answers 404 with <c>TLD_CONFIG_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Get_UnknownId_Returns404WithTldConfigNotFoundAsync()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/admin/tld-configs/{DomainsTestSeeds.UnknownId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.TLD_CONFIG_NOT_FOUND);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task Get_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/admin/tld-configs/{DomainsTestSeeds.UnknownId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
