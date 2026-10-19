using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// <c>POST /api/admin/storefront/upload-image</c>: the picture upload behind the storefront content editors runs the
/// branding pipeline under the content kind — a decodable image is re-encoded and stored under its own
/// directory, bytes that are not an image are refused, and the route is closed to a caller with no credential.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UploadStorefrontImageTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A minimal, genuinely decodable 1x1 transparent PNG.</summary>
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

    /// <summary>Builds a multipart body holding one file.</summary>
    /// <param name="bytes">The file's bytes.</param>
    /// <param name="fileName">The untrusted file name.</param>
    /// <returns>The multipart content.</returns>
    private static MultipartFormDataContent Upload(byte[] bytes, string fileName)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "file", fileName);
        return content;
    }

    /// <summary>A decodable image is stored under the content directory and answers with its URL.</summary>
    [Fact]
    public async Task ValidPng_ReturnsAUrlUnderTheContentDirectory()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        using var content = Upload(TinyPng, "avatar.png");

        var response = await client.PostAsync("/api/admin/storefront/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("url").GetString().Should().StartWith("/uploads/branding/content/").And.EndWith(".png");
    }

    /// <summary>Bytes that do not decode as an image are refused with <c>BRANDING_IMAGE_INVALID</c>.</summary>
    [Fact]
    public async Task UndecodableBytes_RefuseInvalid()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        using var content = Upload("not an image"u8.ToArray(), "avatar.png");

        var response = await client.PostAsync("/api/admin/storefront/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.BRANDING_IMAGE_INVALID);
    }

    /// <summary>A request with no file is refused as a validation failure, not a server error.</summary>
    [Fact]
    public async Task NoFile_RefusesAsValidationFailure()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/api/admin/storefront/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        using var content = Upload(TinyPng, "avatar.png");

        var response = await factory.CreateClient().PostAsync("/api/admin/storefront/upload-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
