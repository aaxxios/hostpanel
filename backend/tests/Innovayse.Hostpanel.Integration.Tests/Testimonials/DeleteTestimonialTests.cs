using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Testimonials;

/// <summary>
/// Integration tests for the <c>DeleteTestimonial</c> use case — <c>DELETE /api/admin/testimonials/{id}</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteTestimonialTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deleting removes the testimonial: a later read of the id answers 404.</summary>
    [Fact]
    public async Task Delete_RemovesTheRecord()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        var id = await StorefrontContentTestSeeds.CreateAsync(client, "/api/admin/testimonials", StorefrontContentTestSeeds.TestimonialBody());

        var response = await client.DeleteAsync($"/api/admin/testimonials/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await client.GetAsync($"/api/admin/testimonials/{id}");
        afterwards.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>An id that names no testimonial answers 404 with the not-found code.</summary>
    [Fact]
    public async Task Delete_UnknownId_RefusesWithNotFoundCode()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync("/api/admin/testimonials/2147483000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.TESTIMONIAL_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Delete_RefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync("/api/admin/testimonials/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
