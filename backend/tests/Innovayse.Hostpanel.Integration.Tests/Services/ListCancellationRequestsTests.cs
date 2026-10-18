using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>GET /api/admin/cancellation-requests</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListCancellationRequestsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Admin reads the paged list of cancellation requests without a refusal.</summary>
    [Fact]
    public async Task GetAll_AsAdmin_Returns200()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/admin/cancellation-requests");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Reading the list without authentication is refused.</summary>
    [Fact]
    public async Task GetAll_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/cancellation-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client calling the admin list route is refused -- the policy is admin only.</summary>
    [Fact]
    public async Task GetAll_Returns403_AsPlainClientAsync()
    {
        var (client, _) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "list-cancel-forbidden");

        var response = await client.GetAsync("/api/admin/cancellation-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
