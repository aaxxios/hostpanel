using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>RegisterDomain</c> use case —
/// <c>POST /api/domains/register</c>, admin/reseller registration of a new domain for a client.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RegisterDomainTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Admin POSTs to <c>/api/domains/register</c> with a valid body. Expects 201 Created and a
    /// Location header pointing to the new domain.
    /// </summary>
    [Fact]
    public async Task Register_ValidRequest_Returns201Async()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);
        var (_, clientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "register-domain");

        var domainName = $"test-{Guid.NewGuid():N}";
        var response = await admin.PostAsJsonAsync("/api/domains/register", new
        {
            // The action binds RegisterDomainCommand itself, so the body carries the command's
            // member names; the registrant block a deleted request record once required is
            // never read.
            domainName = $"{domainName}.com",
            years = 1,
            clientId,
            whoisPrivacy = false,
            autoRenew = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    /// <summary>An unauthenticated caller is refused with 401 before anything is registered.</summary>
    [Fact]
    public async Task Register_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/domains/register", new
        {
            domainName = $"{Guid.NewGuid():N}.com",
            years = 1,
            clientId = 1,
            whoisPrivacy = false,
            autoRenew = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
