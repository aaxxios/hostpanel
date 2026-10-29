using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.SetupService;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// <see cref="SetupServiceCommand"/> places the account on a server from the product's server
/// group. It used to pick from every server of the module, so a product tied to one server (for
/// example the only one with SSH key management) was set up on the default server instead.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetupServiceServerGroupTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A product in a group is set up on that group's server, not on the default one.</summary>
    [Fact]
    public async Task SetupService_ProductWithServerGroupIsSetUpOnTheGroupsServer()
    {
        var host = ProvisioningTestSeeds.WithRecordingProvider(factory, new RecordingProvisioningProvider());
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        // The grouped server is the worse pick on its own (a one-account limit loses the capacity
        // tie-break to an unlimited one), so only the group can be why it is chosen.
        var outside = NewServer("outside", isDefault: true, maxAccounts: null);
        var grouped = NewServer("grouped", isDefault: false, maxAccounts: 1);
        context.Servers.AddRange(outside, grouped);
        var group = ServerGroup.Create($"group-{Guid.NewGuid():N}", ServerFillType.LeastFull);
        context.ServerGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);
        grouped.AssignToGroup(group.Id);

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        var productGroup = ProductGroup.Create($"setup-group-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(productGroup);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(productGroup.Id, "Grouped Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);
        await context.Products.Where(p => p.Id == product.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.ServerGroupId, (int?)group.Id));

        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var outcome = await bus.InvokeAsync<Result>(
            new SetupServiceCommand(service.Id, "grouped.example.test", "grouped1", "Passw0rd!"), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue(outcome.ErrorCode);
        var saved = await context.ClientServices.AsNoTracking().SingleAsync(s => s.Id == service.Id);
        saved.ServerId.Should().Be(grouped.Id);
    }

    /// <summary>Builds a CWP7 server.</summary>
    /// <param name="label">A label for the name.</param>
    /// <param name="isDefault">Whether it is the module's default server.</param>
    /// <param name="maxAccounts">The account limit, or <see langword="null"/> for none.</param>
    /// <returns>The unsaved server.</returns>
    private static Server NewServer(string label, bool isDefault, int? maxAccounts) => Server.Create(new ServerDetails(
        Name: $"{label}-{Guid.NewGuid():N}",
        Hostname: $"{label}.example.test",
        IpAddress: null,
        AssignedIpAddresses: null,
        Module: ServerModule.Cwp7,
        Username: "api",
        Password: "secret",
        ApiToken: null,
        AccessHash: "hash",
        UseSSL: true,
        MaxAccounts: maxAccounts,
        IsDefault: isDefault,
        IsDisabled: false,
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
}
