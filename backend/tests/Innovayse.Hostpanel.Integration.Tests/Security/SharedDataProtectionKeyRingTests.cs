using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Security;

/// <summary>
/// The Data Protection key ring lives in the database, so two hosts on one database read each
/// other's protected payloads: a two-factor challenge issued by one instance, or before a redeploy,
/// is still readable by the instance that receives the code.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host the two hosts are derived from.</param>
public sealed class SharedDataProtectionKeyRingTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The purpose both hosts protect and unprotect under.</summary>
    private const string Purpose = "Hostpanel.Tests.SharedKeyRing";

    /// <summary>The payload the first host protects.</summary>
    private const string Payload = "shared-key-ring-payload";

    /// <summary>
    /// A payload protected by one host is unprotected by a second host with its own service
    /// provider and its own in-memory key cache, and the key both used is a row in the database.
    /// </summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task PayloadProtectedByOneHost_IsUnprotectedByASecondHostOnTheSameDatabase()
    {
        using var first = SeparateHost();
        using var second = SeparateHost();

        var token = first.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector(Purpose)
            .Protect(Payload);

        var read = second.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector(Purpose)
            .Unprotect(token);

        read.Should().Be(Payload);
        using var scope = second.Services.CreateScope();
        var keys = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .DataProtectionKeys
            .AsNoTracking()
            .CountAsync();
        keys.Should().BePositive("the key ring is persisted in the database, not on the host");
    }

    /// <summary>
    /// Derives a host of its own from the shared factory: same configuration and database, a
    /// separate service provider, so nothing in memory is shared with any other derived host.
    /// </summary>
    /// <returns>The derived host; the caller disposes it.</returns>
    private WebApplicationFactory<Program> SeparateHost()
    {
        return factory.WithWebHostBuilder(_ => { });
    }
}
