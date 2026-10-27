using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>Default <see cref="ISshServiceContextResolver"/>.</summary>
public sealed class SshServiceContextResolver : ISshServiceContextResolver
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Initialises the resolver.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory.</param>
    public SshServiceContextResolver(IHostpanelDbContext context, ISshKeyProviderFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<SshServiceContext?> ResolveAsync(int serviceId, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == serviceId, ct);
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

        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serverId, ct);
        if (server is null || server.IsDisabled)
        {
            return new SshServiceContext(service, SshAccessState.NotConfigured, null, null, null, null);
        }

        var account = new SshAccount(server.Id, username);

        if (!_factory.Supports(server.Module))
        {
            return new SshServiceContext(service, SshAccessState.Unsupported, server, null, null, account);
        }

        var settings = await _context.ServerSshSettings.AsNoTracking().FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);
        if (settings is null || !settings.IsEnabled || !settings.IsComplete)
        {
            return new SshServiceContext(service, SshAccessState.NotConfigured, server, null, null, account);
        }

        var provider = _factory.CreateFor(server, settings);
        return provider is null
            ? new SshServiceContext(service, SshAccessState.Unsupported, server, null, null, account)
            : new SshServiceContext(service, SshAccessState.Available, server, settings, provider, account);
    }
}
