using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerConnection;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Provisioning.Extensions;

/// <summary>
/// Finds the server a client service is hosted on. Every provisioning command, query and event
/// handler that talks to the hosting provider starts from this lookup, so it is written once here.
/// Servers belong to the Admin module, so the lookup goes through its
/// <see cref="GetServerConnectionQuery"/>.
/// </summary>
public static class ServiceServerExtensions
{
    /// <summary>Reads the connection details of the server a service is assigned to.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the server query through.</param>
    /// <param name="service">The service whose host is wanted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The server, or <c>null</c> when the service has no server assigned or the assigned server no
    /// longer exists.
    /// </returns>
    public static async Task<ServerConnectionDto?> FindServerAsync(
        this IMessageBus bus, ClientService service, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);

        return await bus.FindServerAsync(service.ServerId, ct);
    }

    /// <summary>Reads the connection details of a server by its id, when there is one.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the server query through.</param>
    /// <param name="serverId">The server's id, or <c>null</c> when the service has none assigned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The server, or <c>null</c> when no id is given or the server no longer exists.</returns>
    public static async Task<ServerConnectionDto?> FindServerAsync(
        this IMessageBus bus, int? serverId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        if (serverId is null)
        {
            return null;
        }

        var result = await bus.InvokeAsync<Result<ServerConnectionDto>>(new GetServerConnectionQuery(serverId.Value), ct);

        return result.Value;
    }
}
