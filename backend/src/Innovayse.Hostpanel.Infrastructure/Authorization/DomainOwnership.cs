using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Authorization;

/// <summary>
/// Resolves domain ownership against the client account the current credential acts for.
/// </summary>
public sealed class DomainOwnership : IDomainOwnership
{
    /// <summary>The data context the domain is read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The data context the domain is read from.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public DomainOwnership(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int domainId, CancellationToken ct)
    {
        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsSuccess)
        {
            // Read-only, and only the owning account is compared, so no children are loaded.
            var owned = await _context.Domains.AsNoTracking()
                .AnyAsync(x => x.Id == domainId && x.ClientId == clientId.Value, ct);
            if (owned)
            {
                return Result.Success();
            }
        }

        // A domain that does not exist, a domain belonging to somebody else, and a caller with no
        // account all land here and answer identically. Distinguishing them would turn this
        // route into a way of enumerating ids, and domain ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
    }
}
