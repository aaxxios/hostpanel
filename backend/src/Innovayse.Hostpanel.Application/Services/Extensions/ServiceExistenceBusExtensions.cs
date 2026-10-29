using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Extensions;

/// <summary>
/// Answers whether the client service an id taken from input points at exists, asked of the Services
/// module so a caller in another module never reads its tables.
/// </summary>
public static class ServiceExistenceBusExtensions
{
    /// <summary>Whether a client service with this id exists.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the query through.</param>
    /// <param name="serviceId">The id to look for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when the service exists.</returns>
    public static async Task<bool> ServiceExistsAsync(this IMessageBus bus, int serviceId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        var states = await bus.GetServiceBillingStatesAsync([serviceId], ct);

        return states.ContainsKey(serviceId);
    }
}
