using FluentAssertions;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.SyncServerAccountCountsCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary>
/// Integration tests for <see cref="SyncServerAccountCountsCronHandler"/>: it tests connectivity
/// to each non-disabled server and records the result, skipping disabled servers, and never
/// throws past a single server's own failure.
/// </summary>
/// <remarks>
/// A cron under <c>Commands/</c>, dispatched through the real <see cref="IMessageBus"/> the way
/// <c>AutoTerminateSuspendedCronTests</c> does. The one port this platform does not own --
/// <see cref="IServerConnectionTester"/>, a real network call to the hosting server -- is
/// replaced by <see cref="FakeServerConnectionTester"/>.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SyncServerAccountCountsCronTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host whose server-connection tester is <paramref name="tester"/>.</summary>
    /// <param name="tester">The fake tester the host should resolve instead of a real one.</param>
    /// <returns>The isolated host.</returns>
    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> HostWithFakeTester(
        FakeServerConnectionTester tester) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IServerConnectionTester>(_ => tester)));

    /// <summary>Seeds a non-disabled server directly against the database.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <param name="isDisabled">Whether the seeded server should be disabled.</param>
    /// <returns>The seeded server.</returns>
    private static async Task<Server> SeedServerAsync(IHostpanelDbContext context, bool isDisabled)
    {
        var server = Server.Create(new ServerDetails(
            Name: $"sync-srv-{Guid.NewGuid():N}",
            Hostname: "sync.example.test",
            IpAddress: null,
            AssignedIpAddresses: null,
            Module: ServerModule.Cwp7,
            Username: "api",
            Password: "secret",
            ApiToken: null,
            AccessHash: null,
            UseSSL: true,
            MaxAccounts: null,
            IsDefault: false,
            IsDisabled: isDisabled,
            MonthlyCost: 0m,
            Datacenter: null,
            ServerStatusAddress: null,
            Ns1Hostname: null,
            Ns1Ip: null,
            Ns2Hostname: null,
            Ns2Ip: null,
            Ns3Hostname: null,
            Ns3Ip: null,
            Ns4Hostname: null,
            Ns4Ip: null,
            Ns5Hostname: null,
            Ns5Ip: null));
        context.Servers.Add(server);
        await context.SaveChangesAsync(CancellationToken.None);
        return server;
    }

    /// <summary>A non-disabled server is tested and its connection result recorded.</summary>
    [Fact]
    public async Task NonDisabledServer_IsTestedAndItsConnectionResultRecorded()
    {
        var tester = new FakeServerConnectionTester { AccountsCount = 7 };
        var host = HostWithFakeTester(tester);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var server = await SeedServerAsync(context, isDisabled: false);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new SyncServerAccountCountsCronCommand(), CancellationToken.None);

        tester.TestedServers.Should().ContainSingle(s => s.Id == server.Id);
        var reloaded = await context.Servers.AsNoTracking().FirstAsync(s => s.Id == server.Id, CancellationToken.None);
        reloaded.AccountsCount.Should().Be(7);
    }

    /// <summary>A disabled server is skipped entirely -- never handed to the connection tester.</summary>
    [Fact]
    public async Task DisabledServer_IsSkippedEntirely()
    {
        var tester = new FakeServerConnectionTester();
        var host = HostWithFakeTester(tester);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var server = await SeedServerAsync(context, isDisabled: true);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new SyncServerAccountCountsCronCommand(), CancellationToken.None);

        tester.TestedServers.Should().NotContain(s => s.Id == server.Id);
    }

    /// <summary>A server whose connection test fails is still recorded, not left to crash the run.</summary>
    [Fact]
    public async Task AServerWhoseConnectionTestFails_IsRecordedNotCrashed()
    {
        var tester = new FakeServerConnectionTester { ShouldFail = true };
        var host = HostWithFakeTester(tester);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var server = await SeedServerAsync(context, isDisabled: false);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var act = async () => await bus.InvokeAsync(new SyncServerAccountCountsCronCommand(), CancellationToken.None);

        await act.Should().NotThrowAsync();
        var reloaded = await context.Servers.AsNoTracking().FirstAsync(s => s.Id == server.Id, CancellationToken.None);
        reloaded.AccountsCount.Should().BeNull();
    }
}
