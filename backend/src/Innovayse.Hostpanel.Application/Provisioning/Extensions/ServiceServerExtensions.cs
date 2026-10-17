using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Provisioning.Extensions;

/// <summary>
/// Finds the server a client service is hosted on. Every provisioning command, query and event
/// handler that talks to the hosting provider starts from this lookup, so it is written once here.
/// </summary>
public static class ServiceServerExtensions
{
    /// <summary>Loads the server a service is assigned to, tracked so a caller may update it.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="service">The service whose host is wanted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The server, or <c>null</c> when the service has no server assigned or the assigned server no
    /// longer exists.
    /// </returns>
    public static async Task<Server?> FindServerAsync(
        this IHostpanelDbContext context, ClientService service, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(service);

        if (service.ServerId is not { } serverId)
        {
            return null;
        }

        return await context.Servers.FirstOrDefaultAsync(s => s.Id == serverId, ct);
    }
}
