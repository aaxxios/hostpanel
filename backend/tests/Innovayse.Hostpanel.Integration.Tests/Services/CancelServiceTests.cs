using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.CancelService;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <see cref="CancelServiceCommand"/>: refusing a second open cancellation
/// request, and refusing an unknown service.
/// </summary>
/// <remarks>Moved from the deleted <c>ServiceLifecycleEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CancelServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A second cancellation request while one is already open is refused, not duplicated.</summary>
    [Fact]
    public async Task CancelService_ASecondOpenRequestIsRefused()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var first = await bus.InvokeAsync<Result>(
            new CancelServiceCommand(service.Id, nameof(CancellationType.Immediate), "Changed my mind"), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await bus.InvokeAsync<Result>(
            new CancelServiceCommand(service.Id, nameof(CancellationType.Immediate), null), CancellationToken.None);

        second.IsFailure.Should().BeTrue();
        second.ErrorCode.Should().Be(HostpanelErrorCodes.CANCELLATION_ALREADY_PENDING);
    }

    /// <summary>Cancelling a service that does not exist refuses <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task CancelService_UnknownServiceRefusesNotFound()
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result>(
            new CancelServiceCommand(ServicesTestSeeds.NoSuchServiceId, nameof(CancellationType.Immediate), null), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }
}
