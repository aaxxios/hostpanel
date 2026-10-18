using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Application.Services.Queries.GetCancellationStatus;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <see cref="GetCancellationStatusQuery"/>, the admin-side read that has
/// no refusal: an unknown or never-cancelled service simply answers "not pending".
/// </summary>
/// <remarks>This query has no HTTP route; it is dispatched directly, the way a cron is.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetCancellationStatusTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A service with no cancellation request answers "not pending", not a refusal.</summary>
    [Fact]
    public async Task GetCancellationStatus_NoOpenRequest_AnswersNotPending()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var outcome = await bus.InvokeAsync<Result<CancellationStatusDto>>(
            new GetCancellationStatusQuery(service.Id), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value!.Pending.Should().BeFalse();
    }

    /// <summary>An id that names no service at all also answers "not pending", not a refusal.</summary>
    [Fact]
    public async Task GetCancellationStatus_UnknownService_AlsoAnswersNotPending()
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result<CancellationStatusDto>>(
            new GetCancellationStatusQuery(ServicesTestSeeds.NoSuchServiceId), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value!.Pending.Should().BeFalse();
    }
}
