using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Tests for <c>ServiceSuspendedHandler</c>: a suspended service suspends the hosting account via the provisioning provider.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own (the hosting server's control-panel API) replaced by <see cref="RecordingProvisioningProvider"/>,
/// publishes the event exactly once through the real message bus and asserts the provider call happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ServiceSuspendedHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A suspended service calls the provisioning provider's suspend endpoint exactly once.</summary>
    [Fact]
    public async Task ServiceSuspended_CallsProviderSuspendExactlyOnceAsync()
    {
        var provider = new RecordingProvisioningProvider();
        var host = ProvisioningTestSeeds.WithRecordingProvider(factory, provider);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (clientId, _, serviceId) = await ProvisioningTestSeeds.SeedActiveServiceAsync(context);

        await bus.InvokeAsync(new ServiceSuspendedEvent(serviceId, clientId, "non-payment", DateTimeOffset.UtcNow), CancellationToken.None);

        var call = Assert.Single(provider.Suspended);
        Assert.Equal(serviceId, call.ServiceId);
        Assert.Equal("provisioning-ref-1", call.ProvisioningRef);
        Assert.Equal("non-payment", call.Reason);
    }

    /// <summary>
    /// A service with no server assigned is left alone: the handler has nothing to call the
    /// provider with, and must not throw trying.
    /// </summary>
    [Fact]
    public async Task ServiceSuspended_WithNoServerAssigned_CallsNothingAsync()
    {
        var provider = new RecordingProvisioningProvider();
        var host = ProvisioningTestSeeds.WithRecordingProvider(factory, provider);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (clientId, serviceId) = await ProvisioningTestSeeds.SeedUnassignedServiceAsync(context);

        await bus.InvokeAsync(new ServiceSuspendedEvent(serviceId, clientId, "non-payment", DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.Empty(provider.Suspended);
    }
}
