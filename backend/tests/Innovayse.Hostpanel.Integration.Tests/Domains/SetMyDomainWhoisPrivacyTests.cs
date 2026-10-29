using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the client-facing <c>SetMyDomainWhoisPrivacy</c> use case:
/// <c>PUT /api/me/domains/{id}/whois-privacy</c>. The admin equivalent is <c>SetWhoisPrivacy</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetMyDomainWhoisPrivacyTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>WHOIS privacy toggling refuses a stranger's attempt and applies for the owner.</summary>
    [Fact]
    public async Task SetWhoisPrivacy_RefusesAStrangerAndAppliesForTheOwnerAsync()
    {
        var (_, owner, domainId, stranger) =
            await DomainsTestSeeds.SeedOwnerWithDomainAsync(factory, "set-my-domain-whois-privacy");
        var body = new Dictionary<string, bool> { ["value"] = true };

        var strangersAttempt = await stranger.PutAsJsonAsync($"/api/me/domains/{domainId}/whois-privacy", body);
        strangersAttempt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(strangersAttempt)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);

        var ownersAttempt = await owner.PutAsJsonAsync($"/api/me/domains/{domainId}/whois-privacy", body);
        ownersAttempt.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task SetWhoisPrivacy_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/me/domains/{DomainsTestSeeds.UnknownId}/whois-privacy", new { value = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A non-positive id is refused by the validator with 400 <c>VALIDATION_ERROR</c> before the handler runs.</summary>
    [Fact]
    public async Task SetWhoisPrivacy_WithNonPositiveId_Returns400ValidationErrorAsync()
    {
        var (client, _) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "setmydomainwhoisprivacy-validation");

        var response = await client.PutAsJsonAsync(
            "/api/me/domains/0/whois-privacy",
            new { value = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }
}
