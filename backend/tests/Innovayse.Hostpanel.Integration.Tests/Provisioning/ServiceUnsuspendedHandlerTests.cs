using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Tests for <c>ServiceUnsuspendedHandler</c>: an unsuspended service unsuspends the hosting account via the provisioning provider.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own -- the hosting server's control-panel API -- replaced by <see cref="RecordingProvisioningProvider"/>,
/// publishes the event exactly once through the real message bus and asserts the provider call happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ServiceUnsuspendedHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An unsuspended service calls the provisioning provider's unsuspend endpoint exactly once.</summary>
    [Fact]
    public async Task ServiceUnsuspended_CallsProviderUnsuspendExactlyOnceAsync()
    {
        var provider = new RecordingProvisioningProvider();
        var host = ProvisioningTestSeeds.WithRecordingProvider(factory, provider);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (clientId, _, serviceId) = await ProvisioningTestSeeds.SeedActiveServiceAsync(context);

        await bus.InvokeAsync(new ServiceUnsuspendedEvent(serviceId, clientId), CancellationToken.None);

        var reference = Assert.Single(provider.Unsuspended);
        Assert.Equal("provisioning-ref-1", reference);
    }
}
