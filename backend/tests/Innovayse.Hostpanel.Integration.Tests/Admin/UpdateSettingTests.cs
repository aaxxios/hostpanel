using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// <c>PUT /api/admin/settings/{id}</c>: an admin can change a free-text or a vocabulary-bound
/// setting's value, an unknown id is refused <c>SETTING_NOT_FOUND</c>, an out-of-vocabulary or
/// wrong-shape value is refused <c>INVALID_SETTING_VALUE</c> with a per-key message, a caller with
/// only the Client role is refused, and the route is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>SettingsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateSettingTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An id no seeded or created setting ever takes.</summary>
    private const int NoSuchSettingId = 999_999_999;

    /// <summary>A value from the key's own vocabulary is stored, in its canonical spelling.</summary>
    [Fact]
    public async Task TemplateKey_StoresTheCanonicalSpellingAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.template");

        var update = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "NOVA" });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var read = await client.GetFromJsonAsync<JsonElement>($"/api/admin/settings/{id}");
        read.GetProperty("value").GetString().Should().Be("nova");

        // Leave the deployment's default template as it was for any other test that reads it.
        await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = PortalSettingKeys.TemplateValue(PortalSettingKeys.DefaultTemplate) });
    }

    /// <summary>
    /// A value outside the key's fixed vocabulary answers 400 with the fixed
    /// <c>INVALID_SETTING_VALUE</c> code and the per-key wording under <c>errors[key]</c>.
    /// </summary>
    [Fact]
    public async Task TemplateKey_UnknownValue_RefusesWithDetailAndErrorsAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.template");

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "bootstrap" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.INVALID_SETTING_VALUE);
        body.RootElement.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("errors").GetProperty("portal.template")[0].GetString()
            .Should().Contain("nova");
    }

    /// <summary>A shaped free-text key refuses a value with the wrong shape.</summary>
    [Fact]
    public async Task BrandPrimary_WrongShape_RefusesWithDetailAndErrorsAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.brand.primary");

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "not-a-colour" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.INVALID_SETTING_VALUE);
        body.RootElement.GetProperty("errors").GetProperty("portal.brand.primary")[0].GetString()
            .Should().Contain("#1a73e8");
    }

    /// <summary>Free text with no vocabulary or shape is stored trimmed, unchanged otherwise.</summary>
    [Fact]
    public async Task SiteName_StoresTrimmedFreeTextAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.site.name");

        var update = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "  Acme Hosting  " });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var read = await client.GetFromJsonAsync<JsonElement>($"/api/admin/settings/{id}");
        read.GetProperty("value").GetString().Should().Be("Acme Hosting");

        // Leave the deployment's site name as it was for any other test that reads it.
        await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "Acme Hosting" });
    }

    /// <summary>Updating an unknown id answers 404 with <c>SETTING_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UnknownId_RefusesNotFoundAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{NoSuchSettingId}", new { value = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AdminTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.SETTING_NOT_FOUND);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await AdminTestSeeds.ClientRoleClientAsync(factory, "update-setting-forbidden");

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{NoSuchSettingId}", new { value = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>A caller with no credential is refused with 401.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/admin/settings/{NoSuchSettingId}", new { value = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
