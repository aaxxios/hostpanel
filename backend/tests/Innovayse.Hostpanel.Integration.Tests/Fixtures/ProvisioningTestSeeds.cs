using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Seeding and assertion helpers shared by every <c>Provisioning/&lt;UseCase&gt;Tests.cs</c>
/// class, so each per-use-case file states only what makes it different rather than
/// re-deriving a provisionable service or an admin host with a fake provider. Moved out of the
/// deleted <c>ProvisioningEndpointTests</c>.
/// </summary>
public static class ProvisioningTestSeeds
{
    /// <summary>An id no seeded or created service ever takes.</summary>
    public const int NoSuchServiceId = 999_999_999;

    /// <summary>
    /// Creates a product group, product, client user, and a service order.
    /// Returns the service ID so provisioning tests have a valid target. The service is Pending
    /// and carries no provisioning reference until a test provisions it.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="http">Authenticated HTTP client to use for setup calls.</param>
    /// <param name="adminToken">Admin JWT token for admin-only setup operations.</param>
    /// <returns>The ID of the newly created client service.</returns>
    public static async Task<int> SeedServiceAsync(IntegrationTestFactory factory, HttpClient http, string adminToken)
    {
        var (serviceId, _) = await SeedOwnedServiceAsync(factory, http, adminToken);
        return serviceId;
    }

    /// <summary>
    /// Same as <see cref="SeedServiceAsync"/>, and also returns a bearer token for the client user
    /// who owns the service -- the portal routes under <c>/api/me</c> admit the Client role only, so
    /// a test of the owner's own view must call them as the owner, not as the admin.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="http">Authenticated HTTP client to use for setup calls.</param>
    /// <param name="adminToken">Admin JWT token for admin-only setup operations.</param>
    /// <returns>The ID of the newly created client service and the owner's JWT.</returns>
    public static async Task<(int ServiceId, string OwnerToken)> SeedOwnedServiceAsync(
        IntegrationTestFactory factory, HttpClient http, string adminToken)
    {
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // A server to provision onto. Without one, ServerSelector finds nothing eligible
        // and ProvisionServiceHandler refuses with NO_ELIGIBLE_SERVER -- so every test here
        // failed on setup rather than on what it meant to assert. Nothing is reached over
        // the network: the factory hands the handler a StubProvisioningProviderFactory.
        var serverResponse = await http.PostAsJsonAsync("/api/admin/servers", new
        {
            name = $"Prov-Server-{Guid.NewGuid():N}",
            hostname = "stub-server.example.com",
            module = ServerModule.Cwp7,
            username = "root",
            accessHash = "stub-access-hash",
            isDefault = true,
        });
        serverResponse.EnsureSuccessStatusCode();

        var groupResponse = await http.PostAsJsonAsync("/api/products/groups",
            new { name = $"Prov-Group-{Guid.NewGuid():N}" });
        groupResponse.EnsureSuccessStatusCode();
        var groupId = await groupResponse.Content.ReadFromJsonAsync<int>();

        var productResponse = await http.PostAsJsonAsync("/api/products",
            new
            {
                groupId,
                name = $"Prov-Plan-{Guid.NewGuid():N}",
                type = ProductType.SharedHosting,
                monthlyPrice = 9.99m,
                annualPrice = 99.99m
            });
        productResponse.EnsureSuccessStatusCode();
        var productId = await productResponse.Content.ReadFromJsonAsync<int>();

        var email = $"prov-client-{Guid.NewGuid():N}@example.com";
        http.DefaultRequestHeaders.Authorization = null;

        var registerResponse = await http.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass@123!",
            firstName = "Prov",
            lastName = "Client"
        });
        registerResponse.EnsureSuccessStatusCode();

        var clientToken = await factory.GetClientTokenAsync(email, "Pass@123!");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        HttpResponseMessage? orderResponse = null;
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(300);
            orderResponse = await http.PostAsJsonAsync("/api/me/services", new
            {
                ProductId = productId,
                BillingCycle = BillingCycleNames.Monthly
            });

            if (orderResponse.StatusCode == HttpStatusCode.Created)
            {
                break;
            }
        }

        orderResponse!.StatusCode.Should().Be(HttpStatusCode.Created);
        var serviceId = await orderResponse.Content.ReadFromJsonAsync<int>();

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        return (serviceId, clientToken);
    }

    /// <summary>
    /// Builds an admin client whose host resolves every server's provisioning provider to
    /// <paramref name="provider"/> instead of the always-succeeding host-wide stub.
    /// </summary>
    /// <remarks>
    /// Overrides both <see cref="IProvisioningProviderFactory"/>, which every handler but
    /// <c>GetServiceCredentialsHandler</c> resolves a provider through, and the bare
    /// <see cref="IProvisioningProvider"/> that one handler injects directly -- missing either
    /// leaves that handler still talking to the always-succeeding host-wide stub.
    /// </remarks>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="provider">The fake provider the host should provision through.</param>
    /// <returns>An authenticated admin client.</returns>
    public static async Task<HttpClient> AdminClientWithFakeProviderAsync(
        IntegrationTestFactory factory, FakeProvisioningProvider provider)
    {
        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IProvisioningProviderFactory>(_ => new FakeProvisioningProviderFactory(provider));
                services.AddScoped<IProvisioningProvider>(_ => provider);
            }));

        var token = await factory.GetAdminTokenAsync();
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Clears a service's assigned server directly in the database.</summary>
    /// <remarks>
    /// There is no admin endpoint that provisions a service and then strips its server, and the
    /// domain method that sets a provisioning reference always sets the server alongside it (see
    /// <c>ClientService.ActivateOn</c>) -- on purpose, so every real caller keeps the two together.
    /// Reaching <c>SERVICE_NO_SERVER_ASSIGNED</c> therefore needs a row not attainable through the
    /// API, which is what this helper builds by writing around the aggregate directly.
    /// </remarks>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="serviceId">The service to strip.</param>
    public static async Task ClearAssignedServerAsync(IntegrationTestFactory factory, int serviceId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        await context.ClientServices
            .Where(s => s.Id == serviceId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ServerId, (int?)null));
    }

    /// <summary>Reads the <c>type</c> field of a refusal answered by <c>BaseApiController</c>.</summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The error code.</returns>
    public static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("type").GetString();
    }

    /// <summary>
    /// Builds a host of its own, with the provisioning provider replaced by
    /// <paramref name="provider"/>, so the fake a test reads back is the one the real handler chain
    /// actually called.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host to derive from.</param>
    /// <param name="provider">The recording provider the host's provider factory will hand out.</param>
    /// <returns>The isolated host.</returns>
    public static WebApplicationFactory<Program> WithRecordingProvider(
        IntegrationTestFactory factory, RecordingProvisioningProvider provider) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IProvisioningProviderFactory>(_ => new RecordingProvisioningProviderFactory(provider))));

    /// <summary>Seeds a client, a server and an active hosting service assigned to that server.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The seeded client id, server id and service id.</returns>
    public static async Task<(int ClientId, int ServerId, int ServiceId)> SeedActiveServiceAsync(IHostpanelDbContext context)
    {
        var (client, product) = await SeedClientAndProductAsync(context);

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

        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        service.ActivateOn(server.Id, "provisioning-ref-1");
        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        return (client.Id, server.Id, service.Id);
    }

    /// <summary>Seeds a client and a hosting service that has no server assigned.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The seeded client id and service id.</returns>
    public static async Task<(int ClientId, int ServiceId)> SeedUnassignedServiceAsync(IHostpanelDbContext context)
    {
        var (client, product) = await SeedClientAndProductAsync(context);
        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);
        return (client.Id, service.Id);
    }

    /// <summary>Seeds a client and a shared-hosting product, the two rows a service is created against.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The saved client and product.</returns>
    private static async Task<(Client Client, Product Product)> SeedClientAndProductAsync(IHostpanelDbContext context)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);

        var group = ProductGroup.Create($"provisioning-events-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);
        return (client, product);
    }
}
