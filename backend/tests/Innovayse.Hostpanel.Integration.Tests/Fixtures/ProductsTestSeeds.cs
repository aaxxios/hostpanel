using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Seeding and authentication helpers shared by every Products use-case test file, so the seeding
/// logic exists exactly once rather than once per use case.
/// </summary>
public static class ProductsTestSeeds
{
    /// <summary>A client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The factory whose admin credentials to mint a token from.</param>
    /// <returns>An authenticated client.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Creates a product group through the API and returns its id.</summary>
    /// <param name="client">An authenticated admin client.</param>
    /// <returns>The new group's id.</returns>
    public static async Task<int> CreateGroupAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/products/groups", new { name = $"group-{Guid.NewGuid():N}" });
        return await response.Content.ReadFromJsonAsync<int>();
    }

    /// <summary>Creates a product (in a fresh group) through the API and returns its id.</summary>
    /// <param name="client">An authenticated admin client.</param>
    /// <returns>The new product's id.</returns>
    public static async Task<int> CreateProductAsync(HttpClient client)
    {
        var groupId = await CreateGroupAsync(client);

        var product = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name = "Featured",
            type = ProductType.SharedHosting,
            monthlyPrice = 1m,
            annualPrice = 10m,
        });
        return await product.Content.ReadFromJsonAsync<int>();
    }

    /// <summary>Reads back a created product's admin-grid view, including unsellable rows.</summary>
    /// <param name="client">An authenticated admin client.</param>
    /// <param name="productId">The product to find.</param>
    /// <returns>The product's JSON element from the admin list.</returns>
    public static async Task<JsonElement> GetProductAsync(HttpClient client, int productId)
    {
        var list = await client.GetFromJsonAsync<JsonElement[]>("/api/products?includeUnsellable=true");
        return list!.Single(p => p.GetProperty("id").GetInt32() == productId);
    }

    /// <summary>Asserts the refusal carries the expected error code, which is what a client branches on.</summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    public static async Task AssertCodeAsync(HttpResponseMessage response, string expected)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(expected);
    }
}
