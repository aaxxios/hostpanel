using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Ssl;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the infrastructure the admin panel lists: server groups with servers, and certificate
/// check results. Hostnames sit under the reserved <c>.example</c> suffix and credentials are
/// placeholders; the demo adapters never use either.
/// </summary>
internal sealed class DemoServerSeeder
{
    /// <summary>The nameserver pair every server advertises.</summary>
    private const string PrimaryNameserver = "ns1.demo-dns.example";

    /// <summary>The second nameserver every server advertises.</summary>
    private const string SecondaryNameserver = "ns2.demo-dns.example";

    /// <summary>The data context the rows are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the rows are added to and saved through.</param>
    public DemoServerSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Seeds servers and certificate checks that are not there yet.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        if (!await _db.ServerGroups.AnyAsync(ct))
        {
            await SeedServersAsync(ct);
        }

        if (!await _db.SslChecks.AnyAsync(ct))
        {
            await SeedSslChecksAsync(ct);
        }
    }

    /// <summary>Seeds two server groups with a few servers each.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedServersAsync(CancellationToken ct)
    {
        var shared = ServerGroup.Create("Shared Hosting Cluster", ServerFillType.LeastFull);
        var vps = ServerGroup.Create("VPS Cluster", ServerFillType.FillUntilFull);
        _db.ServerGroups.Add(shared);
        _db.ServerGroups.Add(vps);
        await _db.SaveChangesAsync(ct);

        // Every hosting product maps to the same server module, so a server of that module is what the selector can
        // pick when a service is set up; the variety is only there to fill the admin lists.
        (string Name, string Host, string Ip, ServerModule Module, int MaxAccounts, bool IsDefault, decimal Cost, string Datacenter, int GroupId)[] servers =
        [
            ("Shared EU-01", "shared01.demo-hosting.example", "192.0.2.10", ServerModule.Cwp7, 500, true, 150m, "Frankfurt", shared.Id),
            ("Shared US-01", "shared02.demo-hosting.example", "192.0.2.11", ServerModule.Cwp7, 500, false, 150m, "New York", shared.Id),
            ("VPS EU-01", "vps01.demo-hosting.example", "192.0.2.20", ServerModule.Cwp7, 100, false, 350m, "Frankfurt", vps.Id),
        ];

        for (var i = 0; i < servers.Length; i++)
        {
            var (name, host, ip, module, max, isDefault, cost, datacenter, groupId) = servers[i];
            var server = Server.Create(new ServerDetails(
                name, host, ip, null, module, "root", "demo-placeholder", null, "demo-placeholder-hash",
                true, max, isDefault, false, cost, datacenter, null,
                PrimaryNameserver, "192.0.2.1", SecondaryNameserver, "192.0.2.2",
                null, null, null, null, null, null));
            server.AssignToGroup(groupId);
            server.RecordConnectionTest(true, 12 + (i * 4));
            _db.Servers.Add(server);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Seeds certificate check results spread across the expiry buckets of the report.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedSslChecksAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        (string Domain, bool HasSsl, string? Issuer, DateTimeOffset? ExpiresAt, bool IsActive)[] checks =
        [
            ("brightleaf.com", true, "Demo Certificate Authority", now.AddDays(75), true),
            ("shop-brightleaf.com", true, "Demo Certificate Authority", now.AddDays(21), true),
            ("araratdigital.com", true, "Demo Certificate Authority", now.AddDays(140), true),
            ("laneconsulting.com", true, "Demo Certificate Authority", now.AddDays(310), true),
            ("rinaldidesignstudio.com", true, "Demo Certificate Authority", now.AddDays(55), true),
            ("moriwebworks.com", false, null, null, true),
            ("retired-site.example", false, null, null, false),
        ];

        foreach (var (domain, hasSsl, issuer, expiresAt, isActive) in checks)
        {
            _db.SslChecks.Add(SslCheck.Create(domain, hasSsl, issuer, expiresAt, isActive));
        }

        await _db.SaveChangesAsync(ct);
    }
}
