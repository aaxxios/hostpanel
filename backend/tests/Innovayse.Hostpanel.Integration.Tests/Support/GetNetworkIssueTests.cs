using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/network-issues/{id}</c>: an admin can read a network issue, an id names no issue
/// is refused <c>NETWORKISSUE_NOT_FOUND</c>, and the action is closed to a caller with no
/// credential.
/// </summary>
/// <remarks>Split out of the deleted <c>KnowledgeBaseAndContentEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetNetworkIssueTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can read a network issue.</summary>
    [Fact]
    public async Task AdminCanReadANetworkIssueAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/network-issues", new
        {
            title = $"Issue-{Guid.NewGuid():N}",
            type = "Server",
            server = "web-01",
            priority = nameof(NetworkIssuePriority.High),
            startDate = DateTimeOffset.UtcNow,
            description = "Something is down.",
        });
        var id = int.Parse((await createResponse.Content.ReadAsStringAsync()).Trim());

        var response = await client.GetAsync($"/api/network-issues/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Reading a network issue that does not exist answers 404 with <c>NETWORKISSUE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task GetUnknownNetworkIssueAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync($"/api/network-issues/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.NETWORKISSUE_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot read a network issue.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync($"/api/network-issues/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
