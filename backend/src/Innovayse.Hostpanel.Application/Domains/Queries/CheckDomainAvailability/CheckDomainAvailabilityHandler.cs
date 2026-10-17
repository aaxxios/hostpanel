using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.CheckDomainAvailability;

/// <summary>
/// Checks whether a domain name is available for registration.
/// First checks the local database — if the domain is already registered through us it is not available.
/// Falls back to the registrar provider API for external availability.
/// </summary>
public sealed class CheckDomainAvailabilityHandler
{
    /// <summary>The registrar provider consulted when the name is not already ours.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context consulted first.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider consulted when the name is not already ours.</param>
    /// <param name="context">The data context consulted first.</param>
    public CheckDomainAvailabilityHandler(IRegistrarProvider registrar, IHostpanelDbContext context)
    {
        _registrar = registrar;
        _context = context;
    }

    /// <summary>
    /// Handles <see cref="CheckDomainAvailabilityQuery"/>.
    /// </summary>
    /// <param name="query">The check domain availability query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the domain is available; otherwise <see langword="false"/>. Never
    /// refuses.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(CheckDomainAvailabilityQuery query, CancellationToken ct)
    {
        // If the domain already exists in our system, it's not available
        var existing = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Name == query.Domain, ct);
        if (existing is not null)
        {
            return Result.Success(false);
        }

        var available = await _registrar.CheckAvailabilityAsync(query.Domain, ct);
        return Result.Success(available);
    }
}
