using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetMyDomains;

/// <summary>Returns every domain owned by the calling client as full <see cref="DomainDto"/> items.</summary>
public sealed class GetMyDomainsHandler
{
    /// <summary>The data context the caller's client record and domains are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking; the query does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the caller's client record and domains are read from.</param>
    /// <param name="caller">Who is asking; the query does not say, and must not.</param>
    public GetMyDomainsHandler(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <summary>
    /// Handles <see cref="GetMyDomainsQuery"/>.
    /// </summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// All domains for the client, including nameservers and DNS records, or
    /// <c>CLIENT_PROFILE_NOT_FOUND</c> when no client record exists for the user.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<IReadOnlyList<DomainDto>>> HandleAsync(GetMyDomainsQuery query, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        var client = await _context.Clients
            .IncludeChildren()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (client is null)
        {
            return Result<IReadOnlyList<DomainDto>>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        var domains = await _context.Domains
            .IncludeChildren()
            .Where(x => x.ClientId == client.Id)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        IReadOnlyList<DomainDto> dtos = domains.Select(d => d.ToDto()).ToList();

        return Result.Success(dtos);
    }
}
