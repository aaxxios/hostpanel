using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>TransferDomain</c> use case —
/// <c>POST /api/domains/transfer</c>, initiating an incoming domain transfer for a client. The
/// client-facing equivalent is <c>TransferMyDomain</c>. Had no Integration coverage of its own
/// before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TransferDomainTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A valid transfer request succeeds with 201 and a Location header pointing to the new
    /// domain -- <see cref="StubRegistrarProvider.TransferAsync"/> always answers success.
    /// </summary>
    [Fact]
    public async Task Transfer_ValidRequest_Returns201Async()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);
        var (_, clientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "transfer-domain");

        var response = await admin.PostAsJsonAsync("/api/domains/transfer", new
        {
            clientId,
            domainName = $"{Guid.NewGuid():N}.test",
            eppCode = "EPP-SECRET-CODE",
            whoisPrivacy = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    /// <summary>An EPP code shorter than the validator's minimum length refuses with 400.</summary>
    [Fact]
    public async Task Transfer_WithShortEppCode_Returns400Async()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);
        var (_, clientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "transfer-domain-short-epp");

        var response = await admin.PostAsJsonAsync("/api/domains/transfer", new
        {
            clientId,
            domainName = $"{Guid.NewGuid():N}.test",
            eppCode = "ab",
            whoisPrivacy = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task Transfer_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/domains/transfer", new
        {
            clientId = 1,
            domainName = $"{Guid.NewGuid():N}.test",
            eppCode = "EPP-SECRET-CODE",
            whoisPrivacy = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
