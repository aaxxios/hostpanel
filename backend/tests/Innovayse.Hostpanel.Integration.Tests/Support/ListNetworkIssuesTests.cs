using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/network-issues</c>: an admin can list network issues, and the list is closed to a
/// caller with no credential.
/// </summary>
/// <remarks>New this batch: no grouped file exercised the listing route before.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListNetworkIssuesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list network issues.</summary>
    [Fact]
    public async Task AdminCanListNetworkIssuesAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/network-issues?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The network issue list is closed to a caller with no credential.</summary>
    [Fact]
    public async Task ListIsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/network-issues?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
