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

    /// <summary>An empty or absent value for a secret setting keeps the stored secret.</summary>
    [Fact]
    public async Task SecretKey_EmptyValue_KeepsTheStoredSecretAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await SecretSettingTestSeeds.StoreAsync(factory, "integration:plesk:password", "stored-panel-password");

        var empty = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = string.Empty });
        var absent = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { });

        empty.StatusCode.Should().Be(HttpStatusCode.NoContent);
        absent.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:plesk:password")).Should().Be("stored-panel-password");
    }

    /// <summary>A new value for a secret setting replaces the stored one, and is not echoed back.</summary>
    [Fact]
    public async Task SecretKey_NewValue_ReplacesTheStoredSecretAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await SecretSettingTestSeeds.StoreAsync(factory, "integration:enom:api_key", "old-registrar-key");

        var update = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "new-registrar-key" });

        update.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:enom:api_key")).Should().Be("new-registrar-key");
        var read = await client.GetFromJsonAsync<JsonElement>($"/api/admin/settings/{id}");
        read.GetRawText().Should().NotContain("new-registrar-key");
    }

    /// <summary>An explicit clear removes a secret setting's value.</summary>
    [Fact]
    public async Task SecretKey_Clear_RemovesTheStoredSecretAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await SecretSettingTestSeeds.StoreAsync(factory, "integration:resellerclub:api_key", "reseller-key");

        var update = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { clear = true });

        update.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:resellerclub:api_key")).Should().BeEmpty();
        var read = await client.GetFromJsonAsync<JsonElement>($"/api/admin/settings/{id}");
        read.GetProperty("hasValue").GetBoolean().Should().BeFalse();
    }

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

    /// <summary>The contact email refuses a value that is not an address.</summary>
    [Fact]
    public async Task ContactEmail_NotAnAddress_RefusesWithDetailAndErrorsAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.contact.email");

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "not-an-address" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.INVALID_SETTING_VALUE);
        body.RootElement.GetProperty("errors").GetProperty("portal.contact.email")[0].GetString()
            .Should().Contain("support@example.com");
    }

    /// <summary>The map address refuses a plain http address.</summary>
    [Fact]
    public async Task ContactMapUrl_Http_RefusesAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.contact.map_url");

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "http://maps.example.com/embed" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>A per-language address is free text, stored trimmed like the default one.</summary>
    [Fact]
    public async Task ContactAddressRu_StoresTrimmedFreeTextAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, "portal.contact.address.ru");

        var update = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "  ул. Примерная, 1  " });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var read = await client.GetFromJsonAsync<JsonElement>($"/api/admin/settings/{id}");
        read.GetProperty("value").GetString().Should().Be("ул. Примерная, 1");
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

    /// <summary>A billing key refuses a value outside its pattern with INVALID_SETTING_VALUE.</summary>
    [Fact]
    public async Task BillingKey_BadValue_IsRefusedAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, BillingSettingKeys.InvoiceDaysBefore);

        var response = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "abc" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.INVALID_SETTING_VALUE);
        body.RootElement.GetProperty("errors").GetProperty(BillingSettingKeys.InvoiceDaysBefore)[0].GetString()
            .Should().Contain("0 to 90");
    }

    /// <summary>Clearing the reminder list is allowed (switches reminders off).</summary>
    [Fact]
    public async Task ReminderDays_CanBeClearedAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var id = await AdminTestSeeds.FindSettingIdAsync(client, BillingSettingKeys.ReminderDays);

        var update = await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = string.Empty });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var read = await client.GetFromJsonAsync<JsonElement>($"/api/admin/settings/{id}");
        read.GetProperty("value").GetString().Should().BeEmpty();

        // Restore the seeded default for any other test that reads it.
        await client.PutAsJsonAsync($"/api/admin/settings/{id}", new { value = "7,3,1" });
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
