using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/departments</c>: the department list is reachable with no credential at all -- the
/// ticket form needs it to populate its department picker before the visitor has any admin
/// context.
/// </summary>
/// <remarks>Split out of the deleted <c>DepartmentsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetDepartmentsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The department list is reachable with no credential at all.</summary>
    [Fact]
    public async Task ListIsReachableAnonymouslyAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
