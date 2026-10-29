using System.Net;
using System.Text.Json;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// The local session's CSRF check: a state-changing request that rides on the session cookie alone
/// must carry <c>X-Requested-With</c>; bearer callers and safe methods are never refused by it.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class LocalSessionCsrfTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A state-changing route; the CSRF check runs before authentication reaches it.</summary>
    private const string WriteRoute = "/api/admin/faqs";

    /// <summary>A cookie value; the check looks only at whether the cookie is presented.</summary>
    private const string CookieValue = "not-a-real-token";

    /// <summary>A cookie-only POST without the header is refused 403 with the one localized error body.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CookieWrite_WithoutHeader_IsRefused403Async()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, WriteRoute);
        request.Headers.Add("Cookie", $"{LocalSessionCookie.Name}={CookieValue}");
        request.Headers.Add("X-Language", "ru");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(SharedErrorCodes.CsrfHeaderMissing);
        body.RootElement.GetProperty("detail").GetString().Should().MatchRegex("[А-Яа-яЁё]");
    }

    /// <summary>The same POST with the header passes the check and is answered by authentication instead.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CookieWrite_WithHeader_PassesTheCheckAsync()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, WriteRoute);
        request.Headers.Add("Cookie", $"{LocalSessionCookie.Name}={CookieValue}");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        var response = await client.SendAsync(request);

        await AssertNotCsrfRefusalAsync(response);
    }

    /// <summary>A bearer caller is not examined, even with a cookie present.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task BearerWrite_IsNotRefusedByTheCheckAsync()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, WriteRoute);
        request.Headers.Add("Cookie", $"{LocalSessionCookie.Name}={CookieValue}");
        request.Headers.Add("Authorization", $"{BearerScheme.HeaderPrefix}{CookieValue}");

        var response = await client.SendAsync(request);

        await AssertNotCsrfRefusalAsync(response);
    }

    /// <summary>An Authorization header that carries no bearer token does not exempt a cookie write.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task NonBearerAuthorization_WithCookie_IsRefused403Async()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, WriteRoute);
        request.Headers.Add("Cookie", $"{LocalSessionCookie.Name}={CookieValue}");
        request.Headers.TryAddWithoutValidation("Authorization", "Basic dXNlcjpwYXNz");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(SharedErrorCodes.CsrfHeaderMissing);
    }

    /// <summary>A safe method is never refused by the check.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CookieRead_IsNotRefusedByTheCheckAsync()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, WriteRoute);
        request.Headers.Add("Cookie", $"{LocalSessionCookie.Name}={CookieValue}");

        var response = await client.SendAsync(request);

        await AssertNotCsrfRefusalAsync(response);
    }

    /// <summary>Asserts the response is not the CSRF refusal.</summary>
    /// <param name="response">The response.</param>
    /// <returns>A task that completes when the assertion has run.</returns>
    private static async Task AssertNotCsrfRefusalAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        text.Should().NotContain(SharedErrorCodes.CsrfHeaderMissing);
    }
}
