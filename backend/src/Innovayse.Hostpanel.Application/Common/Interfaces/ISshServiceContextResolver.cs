using Innovayse.Hostpanel.Application.Services.Common;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Resolves owned service → assigned server → provider → hosting account for every customer SSH
/// use case, so the chain is written once and every handler goes through the same checks.
/// </summary>
public interface ISshServiceContextResolver
{
    /// <summary>
    /// Resolves the service's server, settings, provider and account. Must only be called after
    /// <see cref="IServiceOwnership.RequireOwnedByCallerAsync"/> succeeded for the same id — every
    /// handler does that first, which is what the caller-scoped guard tests check. Never calls the
    /// server: <see cref="SshServiceContext.State"/> is either <c>Available</c> (meaning "ask the
    /// server") or the reason not to.
    /// </summary>
    /// <param name="serviceId">The service id from the route.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved context, or <see langword="null"/> when the service does not exist.</returns>
    Task<SshServiceContext?> ResolveAsync(int serviceId, CancellationToken ct);
}
