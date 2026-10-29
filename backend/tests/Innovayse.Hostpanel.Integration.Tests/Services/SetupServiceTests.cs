using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.SetupService;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <see cref="SetupServiceCommand"/>: refusing a service that is not
/// Pending, and refusing one with no eligible server while keeping the stored details.
/// </summary>
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
        var service = ClientService.Create(clientId, productId, BillingCycle.Monthly, DateTimeOffset.UtcNow);
        await ServicesTestSeeds.AddActivatedAsync(context, service, "panel-ref-1", DateTimeOffset.UtcNow);
        await context.SaveChangesAsync(CancellationToken.None);

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
        var service = ClientService.Create(clientId, productId, BillingCycle.Monthly, DateTimeOffset.UtcNow);
        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        var selector = scope.ServiceProvider.GetRequiredService<IServerSelector>();
        (await selector.SelectAsync(ServerModule.Cwp7, CancellationToken.None)).Should().BeNull(
            "the test host registers no server, so this refusal is genuine and not a stale fixture");

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var outcome = await bus.InvokeAsync<Result>(
            new SetupServiceCommand(service.Id, "example.com", "jdoe", "Passw0rd!"), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.NO_ELIGIBLE_SERVER);

        // The details stay stored for the next attempt, and the service stays Pending.
        using var readScope = factory.Services.CreateScope();
        var stored = await readScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>().ClientServices
            .AsNoTracking()
            .SingleAsync(s => s.Id == service.Id);
        stored.Domain.Should().Be("example.com");
        stored.Username.Should().Be("jdoe");
        stored.Status.Should().Be(ServiceStatus.Pending);
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
