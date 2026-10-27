using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>Terminating a service removes the SSH keys HostPanel installed for it.</summary>
public sealed class RevokeSshKeysOnServiceTerminatedHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task Termination_RevokesEveryActiveKeyOnItsServer_AndLeavesOtherServicesAloneAsync()
    {
        var (http, clientId, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-term");
        var other = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverId, "other");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        await SshTestSeeds.AddKeyAsync(http, serviceId, 2);
        await SshTestSeeds.AddKeyAsync(http, other, 3);

        await Publish(new ServiceTerminatedEvent(serviceId, clientId, "test"));

        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("web").Should().BeEmpty();
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("other").Should().HaveCount(1);
        (await StatesAsync(serviceId)).Should().OnlyContain(s => s == SshKeySyncState.Revoked);
    }

    [Fact]
    public async Task Termination_WithTheServerDown_KeepsKeysVisibleAsUnconfirmedAsync()
    {
        var (http, clientId, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-term-down");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        await Publish(new ServiceTerminatedEvent(serviceId, clientId, "test"));

        (await StatesAsync(serviceId)).Should().Equal(SshKeySyncState.RevokeFailed);
    }

    private async Task Publish(ServiceTerminatedEvent evt)
    {
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IMessageBus>().InvokeAsync(evt, CancellationToken.None);
    }

    private async Task<List<SshKeySyncState>> StatesAsync(int serviceId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>().SshKeys.AsNoTracking()
            .Where(k => k.ClientServiceId == serviceId).Select(k => k.SyncState).ToListAsync();
    }
}
