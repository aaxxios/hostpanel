using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Authorization;

/// <summary>
/// Resolves domain ownership against the client the current credential names.
/// </summary>
public sealed class DomainOwnership : IDomainOwnership
{
    /// <summary>The data context the client and the domain are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking. Nothing tells this type whose domains to consider.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The data context the client and the domain are read from.</param>
    /// <param name="caller">Who is asking. Nothing tells this type whose domains to consider.</param>
    public DomainOwnership(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int domainId, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        var client = await _context.Clients
            .IncludeChildren()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (client is not null)
        {
            var domain = await _context.Domains
                .IncludeChildren()
                .FirstOrDefaultAsync(x => x.Id == domainId, ct);
            if (domain is not null && domain.ClientId == client.Id)
            {
                return Result.Success();
            }
        }

        // A domain that does not exist, a domain belonging to somebody else, and a caller with no
        // client record all land here and answer identically. Distinguishing them would turn this
        // route into a way of enumerating ids -- and domain ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
    }
}
