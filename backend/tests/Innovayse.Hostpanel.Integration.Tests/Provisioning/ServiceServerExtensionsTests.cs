using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Common;
using Innovayse.Hostpanel.Application.Provisioning.Extensions;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// <see cref="ServiceServerExtensions"/>, the lookup of the server a service is hosted on that every
/// provisioning command, query and event handler starts from, run against the real PostgreSQL host.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ServiceServerExtensionsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client, a product and a server, and returns the ids a service needs.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The client id, product id and server id.</returns>
    private static async Task<(int ClientId, int ProductId, int ServerId)> SeedAsync(IHostpanelDbContext context)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);

        var group = ProductGroup.Create($"service-server-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        context.Products.Add(product);

        var server = Server.Create(new ServerDetails(
            Name: $"srv-{Guid.NewGuid():N}",
            Hostname: "srv.example.test",
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
        context.Servers.Add(server);
        await context.SaveChangesAsync(CancellationToken.None);

        return (client.Id, product.Id, server.Id);
    }

    /// <summary>A service activated on a server resolves to that server.</summary>
    [Fact]
    public async Task FindServerAsync_ForAnAssignedService_ReturnsTheServerAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId, serverId) = await SeedAsync(context);
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        service.ActivateOn(serverId, "ref-1");
        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        var server = await context.FindServerAsync(service, CancellationToken.None);

        server.Should().NotBeNull();
        server!.Id.Should().Be(serverId);
    }

    /// <summary>A service that was never assigned a server resolves to null.</summary>
    [Fact]
    public async Task FindServerAsync_ForAServiceWithNoServer_ReturnsNullAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId, _) = await SeedAsync(context);
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        var server = await context.FindServerAsync(service, CancellationToken.None);

        server.Should().BeNull();
    }
}
