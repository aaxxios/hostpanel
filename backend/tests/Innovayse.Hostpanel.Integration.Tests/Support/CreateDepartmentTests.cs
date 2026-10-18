using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/departments</c>: an admin can create a support department, and the action is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>DepartmentsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateDepartmentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create a department.</summary>
    [Fact]
    public async Task AdminCanCreateADepartmentAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync("/api/departments", new
        {
            name = $"Sales-{Guid.NewGuid():N}",
            email = "sales@example.com",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>A caller with no credential cannot create a department.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/departments", new
        {
            name = "Anything",
            email = "anything@example.com",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
