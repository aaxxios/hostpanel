using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>DeleteEmailForwardingRule</c> use case —
/// <c>DELETE /api/domains/{id}/email-forwarding/{ruleId}</c>. The full add/update/delete
/// lifecycle lives in <see cref="UpdateEmailForwardingRuleTests"/>; these tests cover this use
/// case's own refusals directly. The client-facing equivalent is
/// <c>DeleteMyDomainEmailForwardingRule</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteEmailForwardingRuleTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deleting a rule on an unknown domain answers 404 with <c>DOMAIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task DeleteEmailForwardingRule_UnknownDomain_Returns404Async()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/domains/{DomainsTestSeeds.UnknownId}/email-forwarding/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unknown rule id on a real domain answers 404 with <c>EMAIL_FORWARDING_RULE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task DeleteEmailForwardingRule_UnknownRule_Returns404WithRuleNotFoundAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var domainId = await DomainsTestSeeds.SeedActiveDomainAsync(context);

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.DeleteAsync(
            $"/api/domains/{domainId}/email-forwarding/{DomainsTestSeeds.UnknownId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.EMAIL_FORWARDING_RULE_NOT_FOUND);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task DeleteEmailForwardingRule_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/domains/{DomainsTestSeeds.UnknownId}/email-forwarding/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
