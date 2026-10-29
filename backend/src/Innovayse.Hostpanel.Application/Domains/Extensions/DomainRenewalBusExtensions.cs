using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Queries.GetDomainRenewalStates;
using Innovayse.Hostpanel.Application.Domains.Queries.GetRenewableDomains;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Extensions;

/// <summary>
/// The shapes recurring billing asks the Domains module's renewal queries in, so every caller
/// dispatches and unwraps them the same way instead of reading the domains tables.
/// </summary>
public static class DomainRenewalBusExtensions
{
    /// <summary>The active, auto-renewing domains expiring on or before <paramref name="dueBy"/>.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the query through.</param>
    /// <param name="clientId">One client's domains, or every client's when <see langword="null"/>.</param>
    /// <param name="dueBy">The end of the window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The due domains, or none when the input failed validation.</returns>
    public static async Task<IReadOnlyList<RenewableDomainDto>> GetRenewableDomainsAsync(
        this IMessageBus bus, int? clientId, DateTimeOffset dueBy, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<RenewableDomainDto>>>(new GetRenewableDomainsQuery(clientId, dueBy), ct);

        return result.Value ?? [];
    }

    /// <summary>The renewal-related state of the named domains.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the query through.</param>
    /// <param name="domainIds">The domains.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The states keyed by domain id; an unknown id is absent.</returns>
    public static async Task<IReadOnlyDictionary<int, DomainRenewalStateDto>> GetDomainRenewalStatesAsync(
        this IMessageBus bus, IReadOnlyList<int> domainIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<DomainRenewalStateDto>>>(new GetDomainRenewalStatesQuery(domainIds), ct);

        return (result.Value ?? []).ToDictionary(d => d.Id);
    }
}
