using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Shared seeding and authentication helpers for the Domains module's per-use-case Integration
/// tests: registering client users, seeding clients and domains directly through
/// <see cref="IHostpanelDbContext"/>, and reading a refusal's error code off a problem+json body.
/// Every <c>Domains/*Tests.cs</c> file calls into this rather than repeating its own copy, so a
/// change to how a test seeds a client or a domain is made once, in one place.
/// </summary>
internal static class DomainsTestSeeds
{
    /// <summary>Shared test password for every user these helpers register.</summary>
    public const string TestPassword = "Pass@123!";

    /// <summary>An id no seed or created row in any Domains test fixture ever takes.</summary>
    public const int UnknownId = 999_999_999;

    /// <summary>Every client-facing domain refusal not owned by the caller answers this code.</summary>
    public const string DomainNotFoundCode = "DOMAIN_NOT_FOUND";

    /// <summary>A client carrying the seeded admin's bearer token, against the shared host.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>An authenticated client.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Registers a brand-new client user and returns an authenticated <see cref="HttpClient"/>
    /// alongside the email it registered with.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short label folded into the unique email, for readable test failures.</param>
    /// <returns>The authenticated client and the email it registered with.</returns>
    public static async Task<(HttpClient Client, string Email)> RegisterClientAsync(
        IntegrationTestFactory factory, string label)
    {
        var httpClient = factory.CreateClient();
        var email = $"{label}-{Guid.NewGuid():N}@example.com";
        var registerResponse = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Domain",
            lastName = "Tester"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await factory.GetClientTokenAsync(email, TestPassword);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (httpClient, email);
    }

    /// <summary>
    /// Polls <c>GET /api/clients/me</c> until Wolverine has created the <c>Client</c> row a fresh
    /// registration raises asynchronously, and returns its numeric id.
    /// </summary>
    /// <param name="client">An authenticated client, freshly registered.</param>
    /// <returns>The numeric client id.</returns>
    public static async Task<int> WaitForClientIdAsync(HttpClient client)
    {
        var clientId = 0;
        for (var i = 0; i < 10 && clientId == 0; i++)
        {
            await Task.Delay(300);
            var meResponse = await client.GetAsync("/api/clients/me");
            if (meResponse.IsSuccessStatusCode)
            {
                var json = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
                if (json.TryGetProperty("id", out var idProp))
                {
                    clientId = idProp.GetInt32();
                }
            }
        }

        clientId.Should().BeGreaterThan(0, "Wolverine should have created the Client record");
        return clientId;
    }

    /// <summary>
    /// Registers a new client user, waits for its <c>Client</c> row, and returns both the
    /// authenticated client and its numeric id -- the "owner" of every ownership test.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short label folded into the unique email, for readable test failures.</param>
    /// <returns>The authenticated client and its numeric client id.</returns>
    public static async Task<(HttpClient Client, int ClientId)> RegisterOwnerAsync(
        IntegrationTestFactory factory, string label)
    {
        var (client, _) = await RegisterClientAsync(factory, label);
        var clientId = await WaitForClientIdAsync(client);
        return (client, clientId);
    }

    /// <summary>
    /// Registers a second client user who owns nothing -- the "somebody else" of every ownership
    /// test. Its own <c>Client</c> row need not exist yet: a caller with no client record refuses
    /// exactly like a caller whose client record does not own the domain.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short label folded into the unique email, for readable test failures.</param>
    /// <returns>An authenticated client for the stranger.</returns>
    public static async Task<HttpClient> RegisterStrangerAsync(IntegrationTestFactory factory, string label)
    {
        var (client, _) = await RegisterClientAsync(factory, label);
        return client;
    }

    /// <summary>Registers a domain for the given client through the admin route.</summary>
    /// <param name="admin">A client authenticated as Admin.</param>
    /// <param name="clientId">The owning client's primary key.</param>
    /// <returns>The new domain's primary key.</returns>
    public static async Task<int> RegisterDomainAsync(HttpClient admin, int clientId)
    {
        var label = Guid.NewGuid().ToString("N")[..12];
        var response = await admin.PostAsJsonAsync("/api/domains/register", new
        {
            domainName = $"{label}.com",
            years = 1,
            clientId,
            whoisPrivacy = false,
            autoRenew = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = response.Headers.Location!.ToString();
        return int.Parse(location.Split('/').Last());
    }

    /// <summary>Sets up an owner with one domain, plus an unrelated stranger client.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short label folded into the owner and stranger's unique emails.</param>
    /// <returns>The admin client, the owner and its domain id, and the stranger.</returns>
    public static async Task<(HttpClient Admin, HttpClient Owner, int DomainId, HttpClient Stranger)>
        SeedOwnerWithDomainAsync(IntegrationTestFactory factory, string label)
    {
        var admin = await AdminClientAsync(factory);
        var (owner, ownerClientId) = await RegisterOwnerAsync(factory, $"{label}-owner");
        var domainId = await RegisterDomainAsync(admin, ownerClientId);
        var stranger = await RegisterStrangerAsync(factory, $"{label}-stranger");
        return (admin, owner, domainId, stranger);
    }

    /// <summary>Parses a problem+json body and reads its <c>type</c> (error code) field.</summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The error code.</returns>
    public static async Task<string> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("type").GetString()!;
    }

    /// <summary>Seeds a client a DB-level (non-HTTP) test's domain is attached to.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The saved client's id.</returns>
    public static async Task<int> SeedClientAsync(IHostpanelDbContext context)
    {
        var client = Client.Create(
            $"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);
        return client.Id;
    }

    /// <summary>Seeds an active, registrar-linked domain for a DB-level (non-HTTP) test to act on.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The saved domain's id.</returns>
    public static async Task<int> SeedActiveDomainAsync(IHostpanelDbContext context)
    {
        var clientId = await SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddYears(1),
            autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-STUB");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);
        return domain.Id;
    }

    /// <summary>Builds the create body for a fresh TLD configuration under a unique extension.</summary>
    /// <returns>The extension used and the anonymous body to post.</returns>
    public static (string Tld, object Body) NewTldBody()
    {
        var tld = $"tld-{Guid.NewGuid():N}"[..12];
        var body = new
        {
            tld,
            registrarModule = "NameAm",
            currency = "AMD",
            sellCurrency = "USD",
            isEnabled = true,
            sortOrder = 1,
            categories = new[] { "test" },
            costRegister = new Dictionary<string, decimal> { ["1"] = 5m },
            costTransfer = new Dictionary<string, decimal> { ["1"] = 5m },
            costRenew = new Dictionary<string, decimal> { ["1"] = 5m },
            sellRegister = new Dictionary<string, decimal> { ["1"] = 10m },
            sellTransfer = new Dictionary<string, decimal> { ["1"] = 10m },
            sellRenew = new Dictionary<string, decimal> { ["1"] = 10m },
        };
        return (tld, body);
    }

    /// <summary>Reads the <c>type</c> field of a refusal answered by <c>BaseApiController</c>.</summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The error code.</returns>
    public static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("type").GetString();
    }

    /// <summary>Seeds an active product a <c>ClientService</c> can be created against.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The new product's id.</returns>
    public static async Task<int> SeedHostingProductAsync(IHostpanelDbContext context)
    {
        var group = ProductGroup.Create($"domain-events-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(
            group.Id, "Starter Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);
        return product.Id;
    }
}
