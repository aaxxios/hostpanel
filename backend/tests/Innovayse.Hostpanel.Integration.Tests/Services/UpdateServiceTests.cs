using Innovayse.Hostpanel.Application.Common.Errors;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.UpdateService;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>PUT /api/services/{id}</c> and <see cref="UpdateServiceHandler"/>.
/// </summary>
/// <remarks>Moved from the deleted <c>ServiceLifecycleEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// PUT with a status this deployment does not know never reaches the handler:
    /// <c>UpdateServiceValidator</c> refuses it first, on the bus, as a plain 400
    /// <c>VALIDATION_ERROR</c> -- the shape every FluentValidation refusal answers with.
    /// </summary>
    [Fact]
    public async Task Update_WithAnUnknownStatus_Returns400ValidationFailed()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var client = await ServicesTestSeeds.AdminClientAsync(factory);
        var response = await client.PutAsJsonAsync($"/api/services/{service.Id}", new
        {
            billingCycle = BillingCycleNames.Monthly,
            recurringAmount = 1m,
            firstPaymentAmount = 1m,
            status = "NoSuchStatus",
            quantity = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }

    /// <summary>
    /// <see cref="UpdateServiceHandler"/>'s own <c>INVALID_SERVICE_STATUS</c> answer is a
    /// defensive one the validator already blocks over HTTP -- proven here the same way
    /// <c>BulkInvoiceActionUnknown</c> in the Transactions sub-batch is: the handler is built and
    /// called directly, bypassing the bus and its validation middleware entirely.
    /// </summary>
    [Fact]
    public async Task UpdateServiceHandler_AnUnknownStatusPassedDirectly_AnswersInvalidServiceStatus()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateServiceHandler(context, uow);
        var outcome = await handler.HandleAsync(
            new UpdateServiceCommand(
                service.Id, null, null, null, null, BillingCycleNames.Monthly, 1m, null, null, null,
                false, null, false, null, null, null, 1m, null, null, "NoSuchStatus", null, 1, null),
            CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.INVALID_SERVICE_STATUS);
    }

    /// <summary>Updating a service that does not exist answers 404 <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Update_OnAnUnknownService_Returns404()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}", new
        {
            billingCycle = BillingCycleNames.Monthly,
            recurringAmount = 1m,
            firstPaymentAmount = 1m,
            quantity = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>A valid update to an existing service succeeds with no content.</summary>
    [Fact]
    public async Task Update_OnAnExistingService_Returns204()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var client = await ServicesTestSeeds.AdminClientAsync(factory);
        var response = await client.PutAsJsonAsync($"/api/services/{service.Id}", new
        {
            billingCycle = BillingCycleNames.Monthly,
            recurringAmount = 2m,
            firstPaymentAmount = 2m,
            quantity = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Updating without authentication is refused.</summary>
    [Fact]
    public async Task Update_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}", new
        {
            billingCycle = BillingCycleNames.Monthly,
            recurringAmount = 1m,
            firstPaymentAmount = 1m,
            quantity = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
