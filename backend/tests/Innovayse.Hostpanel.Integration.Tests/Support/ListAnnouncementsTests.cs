using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/announcements</c>: an admin can list every announcement, drafts included, and the
/// action is closed to a caller with no credential.
/// </summary>
/// <remarks>New this batch: no grouped file exercised the admin listing route before.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListAnnouncementsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list announcements.</summary>
    [Fact]
    public async Task AdminCanListAnnouncementsAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/announcements?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A caller with no credential cannot list announcements.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/announcements?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
