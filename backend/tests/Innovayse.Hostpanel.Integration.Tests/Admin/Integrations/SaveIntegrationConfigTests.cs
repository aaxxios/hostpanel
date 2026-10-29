using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>
/// <c>PUT /api/admin/integrations/{slug}</c>: an admin upserts an integration's configuration,
/// re-saving a secret field empty keeps the stored secret, a new value replaces it, an explicit
/// clear removes it, a secret is stored encrypted, an unknown slug answers 404, an undeclared
/// field answers 400, a caller with only the Client role is refused, and the route is closed to a
/// caller with no credential.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SaveIntegrationConfigTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Re-saving with the secret field empty leaves the previously stored secret untouched.</summary>
    [Fact]
    public async Task SaveWithEmptySecret_KeepsTheStoredSecretAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        var slug = "namecheap";

        await client.PutAsJsonAsync($"/api/admin/integrations/{slug}", new
        {
            isEnabled = false,
            config = new Dictionary<string, string>
            {
                ["api_key"] = "original-secret-value",
                ["api_username"] = "someone",
                ["client_ip"] = "1.2.3.4",
            },
        });

        var resave = await client.PutAsJsonAsync($"/api/admin/integrations/{slug}", new
        {
            isEnabled = true,
            config = new Dictionary<string, string>
            {
                ["api_key"] = string.Empty,
                ["api_username"] = "someone-else",
                ["client_ip"] = "1.2.3.4",
            },
        });
        resave.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var test = await client.PostAsync($"/api/admin/integrations/{slug}/test", null);
        var body = await test.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue("the original key is still stored");
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:namecheap:api_key")).Should().Be("original-secret-value");
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:namecheap:api_username")).Should().Be("someone-else");
    }

    /// <summary>A secret field left out of the body keeps the stored value too.</summary>
    [Fact]
    public async Task SaveWithoutTheSecretField_KeepsTheStoredSecretAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        await SecretSettingTestSeeds.StoreAsync(factory, "integration:maxmind:license_key", "kept-licence");

        var save = await client.PutAsJsonAsync("/api/admin/integrations/maxmind", new
        {
            isEnabled = false,
            config = new Dictionary<string, string> { ["account_id"] = "42" },
        });

        save.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:maxmind:license_key")).Should().Be("kept-licence");
    }

    /// <summary>A new value for a secret field replaces the stored one.</summary>
    [Fact]
    public async Task SaveWithNewSecret_ReplacesTheStoredSecretAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        await SecretSettingTestSeeds.StoreAsync(factory, "integration:stripe:secret_key", "old-gateway-key");

        var save = await client.PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string> { ["secret_key"] = "new-gateway-key" },
        });

        save.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:stripe:secret_key")).Should().Be("new-gateway-key");
    }

    /// <summary>An explicit clear removes the stored secret, and the detail then reports it as not set.</summary>
    [Fact]
    public async Task SaveWithClear_RemovesTheStoredSecretAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        await SecretSettingTestSeeds.StoreAsync(factory, "integration:smtp:password", "mail-password");

        var save = await client.PutAsJsonAsync("/api/admin/integrations/smtp", new
        {
            isEnabled = false,
            config = new Dictionary<string, string> { ["password"] = "ignored-because-cleared" },
            clearFields = new[] { "password" },
        });

        save.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:smtp:password")).Should().BeEmpty();
        var detail = await client.GetFromJsonAsync<JsonElement>("/api/admin/integrations/smtp");
        detail.GetProperty("config").GetProperty("password").GetProperty("hasValue").GetBoolean().Should().BeFalse();
    }

    /// <summary>A saved secret is stored encrypted: the raw column differs from the input, the read returns it.</summary>
    [Fact]
    public async Task SaveSecret_StoresItEncryptedAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        var secret = SecretSettingTestSeeds.Sentinel("cpanel-token");

        var save = await client.PutAsJsonAsync("/api/admin/integrations/cpanel", new
        {
            isEnabled = false,
            config = new Dictionary<string, string> { ["api_token"] = secret },
        });

        save.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var raw = await SecretSettingTestSeeds.ReadRawAsync(factory, "integration:cpanel:api_token");
        raw.Should().NotBeNullOrEmpty();
        raw.Should().NotBe(secret).And.NotContain(secret);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:cpanel:api_token")).Should().Be(secret);
    }

    /// <summary>A slug that names neither a built-in integration nor a loaded plugin is refused with 404.</summary>
    [Fact]
    public async Task UnknownSlug_IsNotFoundAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        var slug = $"unknown-{Guid.NewGuid():N}";

        var response = await client.PutAsJsonAsync($"/api/admin/integrations/{slug}", new
        {
            isEnabled = true,
            config = new Dictionary<string, string> { ["anything"] = "value" },
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await SecretSettingTestSeeds.ReadAsync(factory, $"integration:{slug}:anything")).Should().BeNull();
    }

    /// <summary>A field the integration does not declare is refused with 400 and nothing is written.</summary>
    [Fact]
    public async Task UndeclaredField_IsRefusedAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string> { ["not_a_field"] = "value" },
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await SecretSettingTestSeeds.ReadAsync(factory, "integration:stripe:not_a_field")).Should().BeNull();
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await IntegrationsTestSeeds.ClientRoleClientAsync(factory, "save-integration-forbidden");

        var response = await client.PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
