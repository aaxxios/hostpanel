using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Products;

/// <summary>
/// <c>GET /api/products/groups</c> against Postgres: the public happy path, and the query's own
/// default for <c>activeOnly</c>.
/// </summary>
/// <remarks>
/// Split from the deleted <c>ProductGroupsEndpointTests</c>. Pins the query's default: the
/// controller used to force <c>activeOnly = true</c> regardless of
/// <c>GetProductGroupsQuery</c>'s own stated default; binding the command directly instead means
/// the query's default now has to say <c>true</c> itself, which this class proves (A7 Task 6).
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetProductGroupsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET is public: the storefront's catalogue reads it without a session.</summary>
    [Fact]
    public async Task List_ReturnsOk_WithoutAuth()
    {
        var response = await factory.CreateClient().GetAsync("/api/products/groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Omitting <c>activeOnly</c> from the query string still hides a group made inactive: the
    /// query's own default has to be <c>true</c> for a bound record, since a genuinely absent
    /// value binds to the type's default, not to whatever the controller used to force.
    /// </summary>
    [Fact]
    public async Task List_OmittedActiveOnly_HidesAnInactiveGroup()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var created = await client.PostAsJsonAsync(
            "/api/products/groups", new { name = $"inactive-{Guid.NewGuid():N}" });
        var id = await created.Content.ReadFromJsonAsync<int>();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var group = await db.Set<ProductGroup>().FirstAsync(g => g.Id == id);
            group.Deactivate();
            await db.SaveChangesAsync();
        }

        var defaultList = await client.GetFromJsonAsync<JsonElement[]>("/api/products/groups");
        defaultList!.Should().NotContain(g => g.GetProperty("id").GetInt32() == id);

        var fullList = await client.GetFromJsonAsync<JsonElement[]>("/api/products/groups?activeOnly=false");
        fullList!.Should().Contain(g => g.GetProperty("id").GetInt32() == id);
    }
}
