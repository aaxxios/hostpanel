using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Seeding and assertion helpers shared by every <c>Services/&lt;UseCase&gt;Tests.cs</c> class, so
/// each per-use-case file states only what makes it different rather than re-deriving an admin
/// token or a client/product pair. Moved out of the endpoint-grouped files this batch dissolved
/// (<c>ServicesEndpointTests</c>, <c>ServiceLifecycleEndpointTests</c>,
/// <c>MyServicesOwnershipEndpointTests</c>) rather than duplicated into each use-case file that
/// replaced them.
/// </summary>
public static class ServicesTestSeeds
{
    /// <summary>Shared test password for every user this class registers.</summary>
    public const string TestPassword = "Pass@123!";

    /// <summary>An id no seeded or created service ever takes.</summary>
    public const int NoSuchServiceId = 999_999_999;

    /// <summary>A client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>An authenticated client with the Admin role.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Reads the <c>type</c> field of a problem+json refusal.</summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The error code.</returns>
    public static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("type").GetString();
    }

    /// <summary>Seeds a client and an active hosting product, priced only in USD.</summary>
    /// <param name="scope">The DI scope to seed through.</param>
    /// <param name="monthlyPrice">The product's monthly USD price, or <see langword="null"/> for none.</param>
    /// <returns>The client id and product id.</returns>
    public static async Task<(int ClientId, int ProductId)> SeedClientAndProductAsync(
        IServiceScope scope, decimal? monthlyPrice)
    {
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);

        var group = ProductGroup.Create($"services-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await uow.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        if (monthlyPrice is { } price)
        {
            product.SetPrice("USD", BillingCycle.Monthly, price);
        }

        context.Products.Add(product);
        await uow.SaveChangesAsync(CancellationToken.None);

        return (client.Id, product.Id);
    }

    /// <summary>
    /// Registers a new client user, waits for Wolverine to create the corresponding
    /// <c>Client</c> row, and returns an authenticated <see cref="HttpClient"/> alongside its
    /// numeric client id.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short, unique-per-test label folded into the registered email.</param>
    /// <returns>The authenticated client and its numeric client id.</returns>
    public static async Task<(HttpClient Client, int ClientId)> RegisterOwnerAsync(
        IntegrationTestFactory factory, string label)
    {
        var httpClient = factory.CreateClient();
        var email = $"{label}-{Guid.NewGuid():N}@example.com";
        var registerResponse = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Service",
            lastName = "Owner"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await factory.GetClientTokenAsync(email, TestPassword);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var clientId = 0;
        for (var i = 0; i < 10 && clientId == 0; i++)
        {
            await Task.Delay(300);
            var meResponse = await httpClient.GetAsync("/api/clients/me");
            if (meResponse.IsSuccessStatusCode)
            {
                var json = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
                if (json.TryGetProperty("id", out var idProp))
                {
                    clientId = idProp.GetInt32();
                }
            }
        }

        clientId.Should().BeGreaterThan(0, "Wolverine should have created the Client record");
        return (httpClient, clientId);
    }

    /// <summary>
    /// Registers a second client user who owns nothing -- the "somebody else" of every ownership
    /// test.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>An authenticated client for the stranger.</returns>
    public static async Task<HttpClient> RegisterStrangerAsync(IntegrationTestFactory factory)
    {
        var httpClient = factory.CreateClient();
        var email = $"my-service-stranger-{Guid.NewGuid():N}@example.com";
        var registerResponse = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Service",
            lastName = "Stranger"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await factory.GetClientTokenAsync(email, TestPassword);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    /// <summary>Creates a Pending service directly against the database, owned by the given client.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="clientId">The owning client's primary key.</param>
    /// <returns>The new service's primary key.</returns>
    public static async Task<int> SeedPendingServiceAsync(IntegrationTestFactory factory, int clientId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // The client row already exists (created by Wolverine on registration); only the
        // product and the service are seeded here.
        var group = ProductGroup.Create($"my-services-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await uow.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 2m, 20m);
        context.Products.Add(product);
        await uow.SaveChangesAsync(CancellationToken.None);

        var service = Domain.Services.ClientService.Create(clientId, product.Id, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        return service.Id;
    }
}
