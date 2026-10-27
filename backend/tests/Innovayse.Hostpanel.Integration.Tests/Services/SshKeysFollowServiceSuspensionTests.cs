using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Events;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Suspending a service takes its SSH keys off the server and unsuspending puts them back. Control
/// panels suspend by locking the password, which does not stop a key from logging in.
/// </summary>
public sealed class SshKeysFollowServiceSuspensionTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Suspension takes only this service's keys off; unsuspension puts the same ids back.</summary>
    [Fact]
    public async Task Suspension_RemovesKeysFromTheServer_AndUnsuspensionRestoresThemAsync()
    {
        var (http, clientId, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-susp");
        var other = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverId, "other");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        await SshTestSeeds.AddKeyAsync(http, serviceId, 2);
        await SshTestSeeds.AddKeyAsync(http, other, 3);
        var agent = SshTestSeeds.Agents(factory).AgentFor(serverId);
        var installed = agent.KeysOf("web").ToList();

        await Publish(new ClientServiceSuspendedEvent(serviceId, clientId));

        agent.KeysOf("web").Should().BeEmpty();
        agent.KeysOf("other").Should().HaveCount(1);
        (await StatesAsync(serviceId)).Should().Equal(SshKeySyncState.Suspended, SshKeySyncState.Suspended);

        await Publish(new ClientServiceUnsuspendedEvent(serviceId, clientId));

        agent.KeysOf("web").ToList().Should().BeEquivalentTo(installed, "the same keys come back, under the same ids");
        (await StatesAsync(serviceId)).Should().Equal(SshKeySyncState.Installed, SshKeySyncState.Installed);
    }

    /// <summary>An unconfirmed removal leaves the key installed, with the reason.</summary>
    [Fact]
    public async Task Suspension_WithTheServerDown_KeepsTheKeyInstalledAndRecordsWhyAsync()
    {
        var (http, clientId, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-susp-down");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        await Publish(new ClientServiceSuspendedEvent(serviceId, clientId));

        // Still on the server as far as anyone knows: claiming otherwise would hide a working login.
        var key = await KeyAsync(serviceId);
        key.SyncState.Should().Be(SshKeySyncState.Installed);
        key.LastSyncError.Should().Be(SshProviderException.Unreachable);
    }

    /// <summary>An unconfirmed restore is a failed install, with the reason.</summary>
    [Fact]
    public async Task Unsuspension_WithTheServerDown_LeavesTheKeyAsAFailedInstallAsync()
    {
        var (http, clientId, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-unsusp-down");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        await Publish(new ClientServiceSuspendedEvent(serviceId, clientId));
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        await Publish(new ClientServiceUnsuspendedEvent(serviceId, clientId));

        var key = await KeyAsync(serviceId);
        key.SyncState.Should().Be(SshKeySyncState.InstallFailed);
        key.LastSyncError.Should().Be(SshProviderException.Unreachable);
    }

    /// <summary>A still-locked account defers the restore to <see cref="RestoreSuspendedSshKeys"/>.</summary>
    [Fact]
    public async Task Unsuspension_BeforeThePanelLiftedItsLock_KeepsTheKeySuspended_AndTheRetryRestoresItAsync()
    {
        var (http, clientId, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-unsusp-race");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        await Publish(new ClientServiceSuspendedEvent(serviceId, clientId));
        var agent = SshTestSeeds.Agents(factory).AgentFor(serverId);
        agent.FailWith = "account_suspended";

        await Publish(new ClientServiceUnsuspendedEvent(serviceId, clientId));

        // The panel has not unlocked the account yet: a wait, not a failure.
        (await KeyAsync(serviceId)).SyncState.Should().Be(SshKeySyncState.Suspended);

        agent.FailWith = null;
        await Publish(new RestoreSuspendedSshKeys(serviceId, clientId));

        (await KeyAsync(serviceId)).SyncState.Should().Be(SshKeySyncState.Installed);
        agent.KeysOf("web").Should().HaveCount(1);
    }

    /// <summary>Delivers a message through the real bus, in a fresh scope.</summary>
    /// <param name="evt">The message.</param>
    /// <returns>A task that completes once it was handled.</returns>
    private async Task Publish(object evt)
    {
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IMessageBus>().InvokeAsync(evt, CancellationToken.None);
    }

    /// <summary>The service's key states, oldest first.</summary>
    /// <param name="serviceId">The service.</param>
    /// <returns>The states.</returns>
    private async Task<List<SshKeySyncState>> StatesAsync(int serviceId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>().SshKeys.AsNoTracking()
            .Where(k => k.ClientServiceId == serviceId).OrderBy(k => k.Id).Select(k => k.SyncState).ToListAsync();
    }

    /// <summary>The service's only key.</summary>
    /// <param name="serviceId">The service.</param>
    /// <returns>The key, untracked.</returns>
    private async Task<SshKey> KeyAsync(int serviceId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>().SshKeys.AsNoTracking()
            .SingleAsync(k => k.ClientServiceId == serviceId);
    }
}
