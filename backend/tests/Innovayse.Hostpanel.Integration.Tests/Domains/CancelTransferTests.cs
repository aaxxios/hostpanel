using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>CancelTransfer</c> use case —
/// <c>POST /api/domains/{id}/cancel-transfer</c>, cancelling a pending incoming domain transfer.
/// Had no Integration coverage of its own before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CancelTransferTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Cancelling a pending transfer succeeds and cancels the domain aggregate.</summary>
    [Fact]
    public async Task CancelTransfer_PendingTransfer_Returns204Async()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.CreateTransfer(clientId, $"{Guid.NewGuid():N}.test");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.PostAsync($"/api/domains/{domain.Id}/cancel-transfer", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>An unknown domain id answers 404 with <c>DOMAIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task CancelTransfer_UnknownDomain_Returns404Async()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/cancel-transfer", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task CancelTransfer_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/cancel-transfer", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
