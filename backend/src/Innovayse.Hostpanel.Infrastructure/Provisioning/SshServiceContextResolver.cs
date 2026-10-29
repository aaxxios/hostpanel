using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshTarget;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Infrastructure.Provisioning;

/// <summary>
/// Default <see cref="ISshServiceContextResolver"/>. Reads the service from its own module and asks
/// the Admin servers module for the server through <see cref="GetServerSshTargetQuery"/>.
/// </summary>
public sealed class SshServiceContextResolver : ISshServiceContextResolver
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Message bus, for the server lookup owned by the servers module.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Provider factory.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Initialises the resolver.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="bus">Message bus, for the server lookup owned by the servers module.</param>
    /// <param name="factory">Provider factory.</param>
    public SshServiceContextResolver(IHostpanelDbContext context, IMessageBus bus, ISshKeyProviderFactory factory)
    {
        _context = context;
        _bus = bus;
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<SshServiceContext?> ResolveAsync(int serviceId, CancellationToken ct)
    {
        // Read-only: the context only describes the service; no caller changes it through this read.
        var service = await _context.ClientServices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId, ct);
        if (service is null)
        {
            return null;
        }

        if (service.Status is not ServiceStatus.Active)
        {
            return new SshServiceContext(service, SshAccessState.ServiceInactive, null, null, null, null);
        }

        // The hosting account name is the provisioning reference every provider call already uses.
        var username = service.ProvisioningRef;
        if (service.ServerId is not int serverId || string.IsNullOrWhiteSpace(username))
        {
            return new SshServiceContext(service, SshAccessState.NotConfigured, null, null, null, null);
        }

        var lookup = await _bus.InvokeAsync<Result<ServerSshTargetDto>>(new GetServerSshTargetQuery(serverId), ct);
        // A successful lookup always carries its value, so Value is only read once IsFailure is false.
        if (lookup.IsFailure || lookup.Value!.IsDisabled)
        {
            return new SshServiceContext(service, SshAccessState.NotConfigured, null, null, null, null);
        }

        var target = lookup.Value;
        var account = new SshAccount(target.ServerId, username);

        if (!_factory.Supports(target.Module))
        {
            return new SshServiceContext(service, SshAccessState.Unsupported, target, null, null, account);
        }

        if (target.Agent is not { IsEnabled: true, IsComplete: true } agent)
        {
            return new SshServiceContext(service, SshAccessState.NotConfigured, target, null, null, account);
        }

        var provider = _factory.CreateFor(target);
        return provider is null
            ? new SshServiceContext(service, SshAccessState.Unsupported, target, null, null, account)
            : new SshServiceContext(service, SshAccessState.Available, target, agent, provider, account);
    }
}
