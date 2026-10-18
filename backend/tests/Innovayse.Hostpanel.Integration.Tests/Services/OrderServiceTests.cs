using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.OrderService;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <see cref="OrderServiceCommand"/>: pricing from the product when the
/// caller leaves an amount unset, honouring a zero amount from order fulfilment, and refusing an
/// unknown or inactive product.
/// </summary>
/// <remarks>
/// Moved from the deleted <c>ServiceLifecycleEndpointTests</c> and <c>ServicesEndpointTests</c>.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class OrderServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A registered client can order a service and receives 201 Created with the new service ID.
    /// Uses a retry loop to handle asynchronous Wolverine client-creation handler latency.
    /// </summary>
    [Fact]
    public async Task OrderServiceAsync_ReturnsCreated_AsClientAsync()
    {
        var client = factory.CreateClient();

        var adminToken = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var groupResponse = await client.PostAsJsonAsync("/api/products/groups",
            new { name = "Hosting-Test" });
        groupResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var groupId = await groupResponse.Content.ReadFromJsonAsync<int>();

        var productResponse = await client.PostAsJsonAsync("/api/products",
            new { groupId, name = "Starter-Test", type = ProductType.SharedHosting, monthlyPrice = 5.99m, annualPrice = 59.99m });
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = await productResponse.Content.ReadFromJsonAsync<int>();

        var email = $"svc-test-{Guid.NewGuid():N}@example.com";
        client.DefaultRequestHeaders.Authorization = null;

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Pass@123!",
            firstName = "Test",
            lastName = "User"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clientToken = await factory.GetClientTokenAsync(email, "Pass@123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        HttpResponseMessage? orderResponse = null;
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(300);
            orderResponse = await client.PostAsJsonAsync("/api/me/services", new
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
        serviceId.Should().BeGreaterThan(0);
    }

    /// <summary>Order-fulfilment callers pass the snapshotted amount, zero included, and it is honoured.</summary>
    [Fact]
    public async Task OrderService_ZeroAmountsFromFulfilmentAreHonouredNotRefused()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 2.99m);
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(clientId, productId, BillingCycleNames.Monthly, 0m, 0m, "Card"), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue();

        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var service = await context.ClientServices.FirstOrDefaultAsync(
            s => s.Id == outcome.Value, CancellationToken.None);
        service!.FirstPaymentAmount.Should().Be(0m);
        service.RecurringAmount.Should().Be(0m);
    }

    /// <summary>A null amount is priced from the product's own price for that cycle.</summary>
    [Fact]
    public async Task OrderService_NullAmountsReadTheProductsPrice()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 4.5m);
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(clientId, productId, BillingCycleNames.Monthly, null, null), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue();

        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var service = await context.ClientServices.FirstOrDefaultAsync(
            s => s.Id == outcome.Value, CancellationToken.None);
        service!.FirstPaymentAmount.Should().Be(4.5m);
        service.RecurringAmount.Should().Be(4.5m);
    }

    /// <summary>A null amount is refused, not sold for nothing, when the product has no such price.</summary>
    [Fact]
    public async Task OrderService_NullAmountsAreRefusedWhenTheProductHasNoPrice()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: null);
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(clientId, productId, BillingCycleNames.Monthly, null, null), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.PRODUCT_NOT_AVAILABLE);
    }

    /// <summary>An unknown product id refuses <c>PRODUCT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task OrderService_UnknownProductRefusesNotFound()
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        await uow.SaveChangesAsync(CancellationToken.None);

        var outcome = await bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(client.Id, ServicesTestSeeds.NoSuchServiceId, BillingCycleNames.Monthly, 1m, 1m), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.PRODUCT_NOT_FOUND);
    }

    /// <summary>An inactive product refuses <c>PRODUCT_NOT_AVAILABLE</c>, even with amounts supplied.</summary>
    [Fact]
    public async Task OrderService_InactiveProductRefusesNotAvailable()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 2m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product!.Deactivate();
        await uow.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var outcome = await bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(clientId, productId, BillingCycleNames.Monthly, 1m, 1m), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.PRODUCT_NOT_AVAILABLE);
    }

    /// <summary>An unauthenticated caller cannot order through <c>/api/me/services</c>.</summary>
    [Fact]
    public async Task OrderServiceAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/me/services", new { ProductId = 1, BillingCycle = BillingCycleNames.Monthly });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
