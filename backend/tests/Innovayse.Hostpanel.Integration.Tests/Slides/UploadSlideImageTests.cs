using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Slides;

/// <summary>
/// Integration tests for the <c>UploadSlideImage</c> use case — <c>POST /api/admin/slides/upload-image</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UploadSlideImageTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An upload whose media type this deployment does not accept is refused with the type code.</summary>
    [Fact]
    public async Task UploadImage_UnsupportedType_RefusesWithTypeCode()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("not an image"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "note.txt");

        var response = await client.PostAsync("/api/admin/slides/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await SlidesTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.SLIDE_IMAGE_TYPE_INVALID);
    }

    /// <summary>An accepted media type is stored and answers 200 with its URL.</summary>
    [Fact]
    public async Task UploadImage_AcceptedType_ReturnsUrl()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        var response = await client.PostAsync("/api/admin/slides/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("url").GetString().Should().StartWith("/uploads/slides/");
    }

    /// <summary>Uploading with no file at all is refused as a validation failure, not a 500.</summary>
    [Fact]
    public async Task UploadImage_NoFile_RefusesAsValidationFailure()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);
        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/api/admin/slides/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task UploadImage_RefusesAnonymous()
    {
        using var content = new MultipartFormDataContent();

        var response = await factory.CreateClient().PostAsync("/api/admin/slides/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
