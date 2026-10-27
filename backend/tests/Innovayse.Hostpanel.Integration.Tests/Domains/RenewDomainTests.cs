using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.RenewDomain;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Integration.Tests.Billing;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>RenewDomain</c> use case —
/// <c>POST /api/domains/{id}/renew</c>, an outright renewal that calls the registrar at once and
/// raises no invoice. The client-facing equivalent, <c>RenewMyDomain</c>, places a paid order
/// instead. Had no Integration coverage of its own before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RenewDomainTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Renewing an active domain succeeds and extends its expiry date.</summary>
    [Fact]
    public async Task RenewDomain_ActiveDomain_Returns204AndExtendsExpiryAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var domainId = await DomainsTestSeeds.SeedActiveDomainAsync(context);
        var originalExpiry = (await context.Domains.AsNoTracking()
            .FirstAsync(d => d.Id == domainId, CancellationToken.None)).ExpiresAt;

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.PostAsJsonAsync($"/api/domains/{domainId}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domainId, CancellationToken.None);
        reloaded.ExpiresAt.Should().BeAfter(originalExpiry);
    }

    /// <summary>An unknown domain id answers 404 with <c>DOMAIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RenewDomain_UnknownDomain_Returns404Async()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task RenewDomain_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// A domain with no registrar reference was never placed at a registrar, so a renewal is
    /// refused with <c>DOMAIN_NO_REGISTRAR_REF</c> and the expiry stays put; extending it locally
    /// would mark a paid domain renewed while it lapses at the registrar.
    /// </summary>
    [Fact]
    public async Task RenewDomain_WithoutRegistrarRef_Returns400AndKeepsExpiryAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(30),
            autoRenew: false, whoisPrivacy: false);
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);
        var originalExpiry = (await context.Domains.AsNoTracking()
            .FirstAsync(d => d.Id == domain.Id, CancellationToken.None)).ExpiresAt;

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.PostAsJsonAsync($"/api/domains/{domain.Id}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.DOMAIN_NO_REGISTRAR_REF);
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        reloaded.ExpiresAt.Should().Be(originalExpiry);
    }

    /// <summary>
    /// When the registrar renews but reports no expiry, the current expiry is extended by the
    /// renewed years, so an early renewal keeps its remaining days.
    /// </summary>
    [Fact]
    public async Task RenewDomain_RegistrarReportsNoExpiry_ExtendsTheCurrentExpiryAsync()
    {
        var host = factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s => s.AddScoped<IRegistrarProvider, NoExpiryRegistrar>()));
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(200),
            autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-STUB");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);
        var originalExpiry = (await context.Domains.AsNoTracking()
            .FirstAsync(d => d.Id == domain.Id, CancellationToken.None)).ExpiresAt;

        var result = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result>(new RenewDomainCommand(domain.Id, 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        reloaded.ExpiresAt.Should().Be(originalExpiry.AddYears(2));
    }
}

/// <summary>A registrar that renews but, like Name.am's purchase response, reports no expiry.</summary>
internal sealed class NoExpiryRegistrar : TestRegistrar
{
    /// <inheritdoc/>
    protected override Task<RegistrarResult> Renew(DomainRenewalOrder request, StubRegistrarProvider stub, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, request.RegistrarRef, null, null));
}
