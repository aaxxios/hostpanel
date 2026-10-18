using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.SetupService;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <see cref="SetupServiceCommand"/>: refusing a service that is not
/// Pending, and refusing one with no eligible server.
/// </summary>
/// <remarks>Moved from the deleted <c>ServiceLifecycleEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetupServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Setting up a service that is already Active is refused as not Pending.</summary>
    [Fact]
    public async Task SetupService_AnAlreadyActiveServiceRefusesNotPending()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        service.Activate("panel-ref-1");
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var outcome = await bus.InvokeAsync<Result>(
            new SetupServiceCommand(service.Id, "example.com", "jdoe", "Passw0rd!"), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.SERVICE_NOT_PENDING);
    }

    /// <summary>
    /// A Pending service with no server registered at all is refused as having none eligible,
    /// rather than crashing the request.
    /// </summary>
    [Fact]
    public async Task SetupService_NoServerRegisteredRefusesNoEligibleServer()
    {
        using var scope = factory.Services.CreateScope();
        var (clientId, productId) = await ServicesTestSeeds.SeedClientAndProductAsync(scope, monthlyPrice: 1m);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        context.ClientServices.Add(service);
        await uow.SaveChangesAsync(CancellationToken.None);

        var selector = scope.ServiceProvider.GetRequiredService<IServerSelector>();
        (await selector.SelectAsync(ServerModule.Cwp7, CancellationToken.None)).Should().BeNull(
            "the test host registers no server, so this refusal is genuine and not a stale fixture");

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var outcome = await bus.InvokeAsync<Result>(
            new SetupServiceCommand(service.Id, "example.com", "jdoe", "Passw0rd!"), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.NO_ELIGIBLE_SERVER);
    }

    /// <summary>Setting up a service that does not exist refuses <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task SetupService_UnknownServiceRefusesNotFound()
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result>(
            new SetupServiceCommand(ServicesTestSeeds.NoSuchServiceId, "example.com", "jdoe", "Passw0rd!"),
            CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }
}
