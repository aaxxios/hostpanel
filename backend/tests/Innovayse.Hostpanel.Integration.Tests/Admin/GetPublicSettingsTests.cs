using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// <c>GET /api/settings/public</c>: readable by a caller with no credential, narrower than the
/// admin shape -- never carrying an id or a description.
/// </summary>
/// <remarks>Split out of the deleted <c>SettingsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetPublicSettingsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The public read is anonymous and never carries an id or description.</summary>
    [Fact]
    public async Task IsAnonymousAndNarrowerThanTheAdminShapeAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/settings/public");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        rows!.Should().OnlyContain(row => IsNarrowerThanTheAdminShape(row));
    }

    /// <summary>
    /// Whether a public settings row carries neither the id nor the description fields the admin
    /// shape does.
    /// </summary>
    /// <param name="row">One row of the public settings response.</param>
    /// <returns>True when the row has no <c>id</c> and no <c>description</c> property.</returns>
    private static bool IsNarrowerThanTheAdminShape(JsonElement row) =>
        !row.TryGetProperty("id", out _) && !row.TryGetProperty("description", out _);
}
