using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.Integration.Tests.Projects;

/// <summary>
/// Integration tests for the <c>DeleteProject</c> use case: <c>DELETE /api/admin/projects/{id}</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteProjectTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deleting removes the project: a later read of the id answers 404.</summary>
    [Fact]
    public async Task Delete_RemovesTheRecord()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        var id = await StorefrontContentTestSeeds.CreateAsync(client, "/api/admin/projects", StorefrontContentTestSeeds.ProjectBody());

        var response = await client.DeleteAsync($"/api/admin/projects/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await client.GetAsync($"/api/admin/projects/{id}");
        afterwards.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>An id that names no project answers 404 with the not-found code.</summary>
    [Fact]
    public async Task Delete_UnknownId_RefusesWithNotFoundCode()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync("/api/admin/projects/2147483000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PROJECT_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Delete_RefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync("/api/admin/projects/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A non-positive id is refused by the validator with 400 <c>VALIDATION_ERROR</c>.</summary>
    [Fact]
    public async Task Delete_NonPositiveId_RefusesWithValidationError()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync("/api/admin/projects/0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, SharedErrorCodes.ValidationError);
    }
}
